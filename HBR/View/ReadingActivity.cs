using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using HBR.DbContext;
using HBR.Extensions;
using HBR.Model;
using HBR.Model.Entity;
using HBR.Model.NavigationItem;
using IdentityModel.OidcClient;
using Microsoft.EntityFrameworkCore;

namespace HBR.View
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class ReadingActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private Book book;

        private OidcClient oidcClient;
        private HttpClient _apiClient;

        private OverScrollWebView webView;

        private TextView textViewTitle;
        private TextView textViewAuthor;
        private ImageView imageViewCover;

        private IMenu tableMenuLeft;
        private IMenu tableMenuRight;

        private List<ChapterNavigationItem> chapterList;
        private List<BookmarkNavigationItem> bookmarkList;
        private List<NavigationItem> AllNavigationItems => chapterList.Concat(chapterList.SelectMany(c => c.SubChapters)).Select(x => (NavigationItem)x).Concat(bookmarkList).ToList();

        private string loadedSrc;
        private int? currentChapterIndex;
        private int? currentSubChapterIndex;

        private HbrClientDbContext _context;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_reading);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            webView = FindViewById<OverScrollWebView>(Resource.Id.contentWebView);
            webView.OnOverScrollY = LoadNextChapter;

            var drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawerLayout, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawerLayout.AddDrawerListener(toggle);
            toggle.SyncState();

            var navigationViewLeft = FindViewById<NavigationView>(Resource.Id.nav_view_left);
            tableMenuLeft = navigationViewLeft.Menu;
            navigationViewLeft.SetNavigationItemSelectedListener(this);

            var navigationViewRight = FindViewById<NavigationView>(Resource.Id.nav_view_right);
            tableMenuRight = navigationViewRight.Menu;
            navigationViewRight.SetNavigationItemSelectedListener(this);

            var navigationViewHeader = navigationViewLeft.GetHeaderView(0);

            textViewAuthor = navigationViewHeader.FindViewById<TextView>(Resource.Id.text_view_author);
            textViewTitle = navigationViewHeader.FindViewById<TextView>(Resource.Id.text_view_title);
            imageViewCover = navigationViewHeader.FindViewById<ImageView>(Resource.Id.image_view_cover);

            _context = this.CreateContext();

            var bookId = Intent.GetStringExtra(nameof(Book.BookId));
            book = await _context.Books.AsNoTracking().Include(b => b.Bookmarks).Include(b => b.LastPosition).FirstOrDefaultAsync(b => b.BookId == bookId);

            await OpenEpub();
        }

        private async void LoadNextChapter()
        {
            if (currentChapterIndex == null)
                return;
            ChapterNavigationItem nextChapter = null;
            try
            {
                var currentMainChapter = chapterList[currentChapterIndex ?? 0];
                nextChapter = currentMainChapter.SubChapters.ElementAtOrDefault((currentChapterIndex ?? 0) + 1);

                if (nextChapter == null)
                {
                    var nextMainChapter = chapterList.ElementAtOrDefault((currentChapterIndex ?? 0) + 1);

                    nextChapter = nextMainChapter?.SubChapters?.FirstOrDefault();

                    if (nextChapter == null)
                        nextChapter = nextMainChapter;
                }
            }
            catch { }

            if (nextChapter != null)
                await LoadChapterAsync(nextChapter, true);
        }

        protected override async void OnStop()
        {
            try
            {
                UserDialogs.Instance.ShowLoading();

                var bookToUpdate = _context.Books.Include(b => b.LastPosition).FirstOrDefault(b => b.BookId == book.BookId);

                var bookmarkToDelete = bookToUpdate.LastPosition;

                bookToUpdate.LastPosition = CreateBookmarkFromCurrentPosition();

                //await _context.SaveChangesAsync();

                if (bookmarkToDelete != null)
                {
                    _context.BookMarks.Remove(bookmarkToDelete);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {

            }
            finally
            {
                UserDialogs.Instance.HideLoading();
                base.OnStop();
            }
        }

        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if (drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_bookmark)
            {
                OnActionBookmarkClicked();
            }
            return base.OnOptionsItemSelected(item);
        }

        private async Task OnActionBookmarkClicked()
        {
            var promptResult = await UserDialogs.Instance.PromptAsync("Leírás", "Új könyvjelző");

            if (!promptResult.Ok || string.IsNullOrEmpty(promptResult.Text))
                return;

            try
            {
                UserDialogs.Instance.ShowLoading();

                var bookToUpdate = _context.Books.Include(b => b.Bookmarks).FirstOrDefault(b => b.BookId == book.BookId);
                var description = promptResult.Text;

                var bookmark = CreateBookmarkFromCurrentPosition(description, bookToUpdate.BookId);

                bookToUpdate.Bookmarks.Add(bookmark);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await UserDialogs.Instance.AlertAsync(e.Message, e.GetType().Name);
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private async void FabOnClick(object sender, EventArgs eventArgs)
        {
            await OpenEpub();

            //var options = new OidcClientOptions
            //{
            //    Authority = HbrApplication.Authority,
            //    ClientId = HbrApplication.ClientId,
            //    Scope = string.Join(" ", HbrApplication.ApiScopes),
            //    Browser = new ChromeCustomTabsWebView(this),
            //};

            //var client = new OidcClient(options);

            //var result = await client.LoginAsync();
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            var selectedChapter = AllNavigationItems.FirstOrDefault(c => c.MenuItemId == item.ItemId);

            selectedChapter?.OnClickCallback?.Invoke();

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private async Task OpenEpub()
        {
            textViewTitle.Text = book.Title;
            textViewAuthor.Text = book.Author;

            var coverImage = await book.GetCoverAsync();

            if (coverImage != null)
                imageViewCover.SetImageBitmap(coverImage);

            chapterList = book.GetChapterList();
            bookmarkList = book.Bookmarks.Select(b => new BookmarkNavigationItem
            {
                Text = b.Description,
                OnClickCallback = new Action(() => LoadBookmark(b))
            }).ToList();

            tableMenuLeft.Clear();
            tableMenuRight.Clear();

            var navigationItemIndex = 0;
            foreach (var chapter in chapterList)
            {
                if (chapter.SubChapters.Count > 0)
                {
                    var subMenu = tableMenuLeft.AddSubMenu(0, navigationItemIndex, Menu.None, chapter.Text);
                    chapter.MenuItemId = navigationItemIndex++;
                    chapter.OnClickCallback = new Action(async () => await LoadChapterAsync(chapter));

                    foreach (var subChapter in chapter.SubChapters)
                    {
                        subMenu.Add(0, navigationItemIndex, Menu.None, subChapter.Text);
                        subChapter.MenuItemId = navigationItemIndex++;

                        subChapter.OnClickCallback = new Action(async () => await LoadChapterAsync(subChapter));
                    }
                }
                else
                {
                    tableMenuLeft.Add(0, navigationItemIndex, Menu.None, chapter.Text);
                    chapter.MenuItemId = navigationItemIndex++;

                    chapter.OnClickCallback = new Action(async () => await LoadChapterAsync(chapter));
                }
            }


            foreach(var bookmark in bookmarkList)
            {
                tableMenuRight.Add(0, navigationItemIndex, Menu.None, bookmark.Text);
                bookmark.MenuItemId = navigationItemIndex++;
            }

            try
            {
                LoadBookmark(book.LastPosition);
            }
            catch
            {
                chapterList?.FirstOrDefault()?.OnClickCallback?.Invoke();
            }
        }

        private void LoadBookmark(Bookmark bookmark)
        {
            if (bookmark != null)
            {
                var lastMainChapter = chapterList[bookmark.ChapterIndex];

                if (bookmark.SubChapterIndex == null)
                    lastMainChapter?.OnClickCallback?.Invoke();
                else
                    lastMainChapter.SubChapters[bookmark.SubChapterIndex.Value]?.OnClickCallback?.Invoke();

                webView.ScrollY = bookmark.Position;
            }
            else
                chapterList?.FirstOrDefault()?.OnClickCallback?.Invoke();
        }

        private async Task LoadChapterAsync(ChapterNavigationItem chapter, bool forceReload = false)
        {
            var link = chapter.Src.Split("#");
            var src = link[0];
            var anchor = link.Length > 1 ? link[1] : string.Empty;

            if (loadedSrc != src || forceReload)
            {
                var url = book.GetChapterUrl(src);
                webView.LoadUrl(url);

                loadedSrc = src;
            }

            if (!string.IsNullOrEmpty(anchor))
                webView.ScrollToAnchor(anchor);
            else
                webView.ScrollToTop();

            var chapterIndex = chapterList.IndexOf(chapter);
            if (chapterIndex >= 0)
            {
                currentChapterIndex = chapterIndex;
                currentSubChapterIndex = null;
            }
            else
            {
                var mainChapter = chapterList.FirstOrDefault(c => c.SubChapters != null && c.SubChapters.Contains(chapter));
                if (mainChapter == null)
                    return;

                currentChapterIndex = chapterList.IndexOf(mainChapter);
                currentSubChapterIndex = mainChapter.SubChapters.IndexOf(chapter);
            }
        }

        private Bookmark CreateBookmarkFromCurrentPosition(string description = null, string bookId = null)
        {
            var scroll = webView.ScrollY;

            return new Bookmark
            {
                BookmarkId = Guid.NewGuid().ToString(),
                Description = description,
                BookId = bookId,
                ChapterIndex = currentChapterIndex ?? 0,
                SubChapterIndex = currentSubChapterIndex,
                Position = scroll
            };
        }
    }
}


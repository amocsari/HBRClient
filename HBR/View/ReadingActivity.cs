using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using HBR.DbContext;
using HBR.Extensions;
using HBR.Model;
using HBR.Model.Entity;
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

        private WebView webView;

        private TextView textViewTitle;
        private TextView textViewAuthor;
        private ImageView imageViewCover;

        private IMenu tableMenu;

        private List<Chapter> chapterList;
        private List<Chapter> AllChapters { get => chapterList.Concat(chapterList.SelectMany(c => c.SubChapters)).ToList(); }

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

            webView = FindViewById<WebView>(Resource.Id.contentWebView);
            webView.Settings.JavaScriptEnabled = true;

            var drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawerLayout, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawerLayout.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            tableMenu = navigationView.Menu;
            navigationView.SetNavigationItemSelectedListener(this);

            var navigationViewHeader = navigationView.GetHeaderView(0);

            textViewAuthor = navigationViewHeader.FindViewById<TextView>(Resource.Id.text_view_author);
            textViewTitle = navigationViewHeader.FindViewById<TextView>(Resource.Id.text_view_title);
            imageViewCover = navigationViewHeader.FindViewById<ImageView>(Resource.Id.image_view_cover);

            _context = this.CreateContext();

            var bookId = Intent.GetStringExtra(nameof(Book.BookId));
            book = _context.Books.AsNoTracking().FirstOrDefault(b => b.BookId == bookId);

            await OpenEpub();
        }

        protected override async void OnStop()
        {
            var scroll = webView.ScrollY;

            var bookToUpdate = _context.Books.FirstOrDefault(b => b.BookId == book.BookId);

            bookToUpdate.LastChapterIndex = currentChapterIndex;
            bookToUpdate.LastSubChapterIndex = currentSubChapterIndex;
            bookToUpdate.LastPosition = scroll;

            await _context.SaveChangesAsync();

            base.OnPause();
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
            return base.OnOptionsItemSelected(item);
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
            var selectedChapter = AllChapters.FirstOrDefault(c => c.MenuItemId == item.ItemId);

            selectedChapter?.OnClickCallback?.Invoke(false);

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

            tableMenu.Clear();

            var chapterIndex = 0;
            foreach (var chapter in chapterList)
            {
                if (chapter.SubChapters.Count > 0)
                {
                    var subMenu = tableMenu.AddSubMenu(0, chapterIndex, Menu.None, chapter.ChapterTitle);
                    chapter.MenuItemId = chapterIndex++;
                    chapter.OnClickCallback = new Action<bool>(async forceReload => await LoadChapterAsync(chapter, forceReload));

                    foreach (var subChapter in chapter.SubChapters)
                    {
                        subMenu.Add(0, chapterIndex, Menu.None, subChapter.ChapterTitle);
                        subChapter.MenuItemId = chapterIndex++;

                        subChapter.OnClickCallback = new Action<bool>(async forceReload => await LoadChapterAsync(subChapter, forceReload));
                    }
                }
                else
                {
                    tableMenu.Add(0, chapterIndex, Menu.None, chapter.ChapterTitle);
                    chapter.MenuItemId = chapterIndex++;

                    chapter.OnClickCallback = new Action<bool>(async forceReload => await LoadChapterAsync(chapter, forceReload));
                }
            }

            try
            {
                if (book.LastChapterIndex == null)
                    AllChapters?.FirstOrDefault()?.OnClickCallback?.Invoke(true);
                else
                {
                    var lastMainChapter = chapterList[book.LastChapterIndex.Value];

                    if (book.LastSubChapterIndex == null)
                        lastMainChapter?.OnClickCallback?.Invoke(true);
                    else
                        lastMainChapter.SubChapters[book.LastSubChapterIndex.Value]?.OnClickCallback?.Invoke(true);

                    webView.ScrollY = book.LastPosition;
                }
            }
            catch
            {
                AllChapters?.FirstOrDefault()?.OnClickCallback?.Invoke(true);
            }
        }

        private async Task LoadChapterAsync(Chapter chapter, bool forceReload)
        {
            var link = chapter.Src.Split("#");
            var src = link[0];
            var anchor = link.Length > 1 ? link[1] : string.Empty;

            if (loadedSrc != src || forceReload)
            {
                using (var streamReader = book.GetDataSteamReader(src))
                    webView.LoadData(await streamReader.ReadToEndAsync(), "text/html", "utf-8");

                loadedSrc = src;
            }

            if (!string.IsNullOrEmpty(anchor))
                webView.EvaluateJavascript($"document.getElementById(\"{ anchor }\").scrollIntoView(true);", null);
            else
                webView.EvaluateJavascript("document.documentElement.scrollTop = 0;", null);

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
    }
}


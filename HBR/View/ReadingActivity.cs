using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using HBR.Model;
using IdentityModel.OidcClient;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;

namespace HBR.View
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class ReadingActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
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

        protected override void OnCreate(Bundle savedInstanceState)
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
            //FileData fileData = await CrossFilePicker.Current.PickFile(new string[] { ".epub" });
            //if (fileData == null)
            //    return;

            //using (var zipToOpen = new MemoryStream(fileData.DataArray))
            //using (var zipArchive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
            //{
            //    var cont = zipArchive.Entries.FirstOrDefault(e => e.Name == "content.opf");
            //    var docContents = XDocument.Load(cont.Open());
            //    var nameSpaceContents = docContents.Root.Name.Namespace;

            //    var metadata = docContents.Descendants(nameSpaceContents + "metadata").Descendants();

            //    if (textViewAuthor == null)
            //        textViewAuthor = FindViewById<TextView>(Resource.Id.textViewWriter);
            //    if (textViewTitle == null)
            //        textViewTitle = FindViewById<TextView>(Resource.Id.textViewTitle);
            //    if (imageViewCover == null)
            //        imageViewCover = FindViewById<ImageView>(Resource.Id.imageViewCover);

            //    textViewTitle.Text = titleNode?.Value;
            //    textViewAuthor.Text = authorNode?.Value;

            //    chapterList = FindChapterList(tableOfContents);

            //    tableMenu.Clear();
            //}

            //var chapterIndex = 0;
            //foreach (var chapter in chapterList)
            //{
            //    if (chapter.SubChapters.Count > 0)
            //    {
            //        var subMenu = tableMenu.AddSubMenu(0, chapterIndex, Menu.None, chapter.ChapterTitle);
            //        chapter.MenuItemId = chapterIndex++;
            //        chapter.OnClickCallback = new Action<bool>(async forceReload => await LoadChapterAsync(chapter, fileData, forceReload));

            //        foreach (var subChapter in chapter.SubChapters)
            //        {
            //            subMenu.Add(0, chapterIndex, Menu.None, subChapter.ChapterTitle);
            //            subChapter.MenuItemId = chapterIndex++;

            //            subChapter.OnClickCallback = new Action<bool>(async forceReload => await LoadChapterAsync(subChapter, fileData, forceReload));
            //        }
            //    }
            //    else
            //    {
            //        tableMenu.Add(0, chapterIndex, Menu.None, chapter.ChapterTitle);
            //        chapter.MenuItemId = chapterIndex++;

            //        chapter.OnClickCallback = new Action<bool>(async forceReload => await LoadChapterAsync(chapter, fileData, forceReload));
            //    }
            //}

            //AllChapters?.FirstOrDefault()?.OnClickCallback?.Invoke(true);
        }

        private async Task LoadChapterAsync(Chapter chapter, FileData fileData, bool forceReload)
        {
            var link = chapter.Src.Split("#");
            var src = link[0];
            var anchor = link.Length > 1 ? link[1] : string.Empty;

            if (loadedSrc != src || forceReload)
            {
                using (var zipToOpen = new MemoryStream(fileData.DataArray))
                using (var zipArchive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {

                    var dataStream = zipArchive.Entries.FirstOrDefault(e => e.FullName == "OEBPS/" + src);

                    if (dataStream == null)
                        dataStream = zipArchive.Entries.FirstOrDefault(e => e.FullName.Contains(src));

                    if (dataStream != null)
                        using (var streamReader = new StreamReader(dataStream.Open()))
                            webView.LoadData(await streamReader.ReadToEndAsync(), "text/html", "utf-8");
                }

                loadedSrc = src;
            }

            if (!string.IsNullOrEmpty(anchor))
                webView.EvaluateJavascript($"document.getElementById(\"{ anchor }\").scrollIntoView(true);", null);
            else
                webView.EvaluateJavascript("document.documentElement.scrollTop = 0;", null);
        }
    }
}


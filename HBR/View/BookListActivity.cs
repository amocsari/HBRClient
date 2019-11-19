using System;
using System.IO;
using System.IO.Compression;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using HBR.Context;
using HBR.Model.Entity;
using HBR.Extensions;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Microsoft.EntityFrameworkCore;
using Android.Support.V7.Widget;
using HbrClient.Library;
using Acr.UserDialogs;

namespace HBR.View
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class BookListActivity : AppCompatActivity
    {
        private HbrClientDbContext _context;

        private LibraryAdapter adapter = new LibraryAdapter();

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            UserDialogs.Init(this);

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_booklist);

            _context = await ContextHelper.CreateContextAsync();

            var recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView_bookList);
            recyclerView.SetAdapter(adapter);
            adapter.RecyclerView = recyclerView;
            adapter.Context = this;
            recyclerView.SetLayoutManager(new LinearLayoutManager(this));

            var bookList = await _context.Books.AsNoTracking().ToListAsync();
            adapter.AddBooks(bookList);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButton_addBook);
            fab.Click += FabOnClick;
        }

        private async void FabOnClick(object sender, EventArgs e)
        {
            FileData fileData;
            try
            {
                fileData = await CrossFilePicker.Current.PickFile(new string[] { ".epub" });
            }
            catch (FormatException)
            {
                return;
            }

            if (fileData == null)
                return;

            var bookId = Guid.NewGuid().ToString();

            var path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), bookId);
            Directory.CreateDirectory(path);

            ZipFile.ExtractToDirectory(fileData.FilePath, path);

            var book = new Book { BookId = bookId };
            book.FillMetadata();

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            adapter.AddBook(book);
        }

        protected override void OnDestroy()
        {
            _context?.Dispose();
        }
    }
}
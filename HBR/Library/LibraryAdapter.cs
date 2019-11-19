using Acr.UserDialogs;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using HBR.Context;
using HBR.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using static Android.Support.V7.Widget.RecyclerView;
using Resource = HBR.Resource;

namespace HbrClient.Library
{
    public class LibraryAdapter : Adapter
    {
        public List<Book> Library { get; set; }
        public Context Context { get; set; }
        public RecyclerView RecyclerView { get; set; }
        private HbrClientDbContext _dbcontext;

        public LibraryAdapter(List<Book> library, Context context)
        {
            Library = library;
            Context = context;
        }

        public LibraryAdapter()
        {
            Library = new List<Book>();
            _dbcontext = ContextHelper.CreateContext();
        }

        public override int ItemCount => Library.Count;

        public override void OnBindViewHolder(ViewHolder holder, int position)
        {
            var bookViewHolder = holder as BookViewHolder;
            bookViewHolder.AuthorTextView.Text = Library[position].Author;
            bookViewHolder.TitleTextView.Text = Library[position].Title;
        }

        public override ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var libraryItemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.view_library_item, parent, false);

            libraryItemView.Click += OnItemClick;

            BookViewHolder bookViewHolder = new BookViewHolder(libraryItemView);
            bookViewHolder.MenuButtonImageView.Click += OnClick;

            return bookViewHolder;
        }

        private async void OnItemClick(object sender, EventArgs e)
        {
            View view = (View)sender;
            var position = RecyclerView.GetChildAdapterPosition(view);
            var book = Library[position];

            //TODO: open book
        }

        public void OnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;

            var position = RecyclerView.GetChildAdapterPosition((View)view.Parent.Parent.Parent);

            PopupMenu popup = new PopupMenu(view.Context, view);
            popup.Inflate(Resource.Menu.menu_library_item);
            popup.MenuItemClick += async (clickedItem, argument) =>
            {
                if (argument.Item.ItemId == Resource.Id.menuItem_libraryItem_delete)
                {
                    var config = new ConfirmConfig
                    {
                        Title = "Törlés megerősítése",
                        Message = "Biztosan törli ezt a könyvet?",
                        OkText = "Törlés",
                        CancelText = "Mégse"
                    };
                    var result = await UserDialogs.Instance.ConfirmAsync(config);

                    if (result)
                    {
                        var dialog = UserDialogs.Instance.Loading("Loading");

                        var book = Library[position];

                        //TODO: törlés

                        Library.RemoveAt(position);
                        NotifyDataSetChanged();

                        dialog.Dispose();
                    }
                }
                else if (argument.Item.ItemId == Resource.Id.menuItem_libraryItem_details)
                {
                    //TODO: navigálás szerkesztő felületre
                }
                else if (argument.Item.ItemId == Resource.Id.menuItem_libraryItem_removeFromShelf)
                {
                    var config = new ConfirmConfig
                    {
                        Title = "Levétel megerősítése",
                        Message = "Biztosan leveszi ezt a könyvet a listájáról?",
                        OkText = "Igen",
                        CancelText = "Mégse"
                    };
                    var result = await UserDialogs.Instance.ConfirmAsync(config);

                    if (result)
                    {
                        var dialog = UserDialogs.Instance.Loading("Loading");

                        var book = Library[position];

                        //TODO: törlés

                        Library.RemoveAt(position);
                        NotifyDataSetChanged();

                        dialog.Dispose();
                    }
                }
            };
            popup.Show();
        }

        public void AddBook(Book book)
        {
            AddBooks(new List<Book> { book });
        }

        public void AddBooks(List<Book> dto)
        {
            if (Library == null)
                Library = new List<Book>();

            Library.AddRange(dto);
            NotifyDataSetChanged();
        }

        public void UpdateBook(Book newBook)
        {
            var oldBook = Library.FirstOrDefault(b => b.BookId == newBook.BookId);
            oldBook.Author = newBook.Author;
            oldBook.Title = newBook.Title;
            NotifyDataSetChanged();
        }

        public void Clear()
        {
            Library.Clear();
            NotifyDataSetChanged();
        }
    }
}
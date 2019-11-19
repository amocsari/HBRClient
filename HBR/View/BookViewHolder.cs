using Android.Views;
using Android.Widget;
using static Android.Support.V7.Widget.RecyclerView;
using Resource = HBR.Resource;

namespace HbrClient.Library
{
    public class BookViewHolder : ViewHolder
    {
        public TextView TitleTextView { get; set; }
        public TextView AuthorTextView { get; set; }
        public ImageView MenuButtonImageView { get; set; }

        public BookViewHolder(View view) : base(view)
        {
            TitleTextView = view.FindViewById<TextView>(Resource.Id.tv_title);
            AuthorTextView = view.FindViewById<TextView>(Resource.Id.tv_author);
            MenuButtonImageView = view.FindViewById<ImageView>(Resource.Id.image_view_menu);
        }
    }
}
using Android.Graphics;
using HBR.Model;
using HBR.Model.Entity;
using HBR.Model.NavigationItem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HBR.Extensions
{
    public static class BookExtensions
    {
        public static Book FillMetadata(this Book book)
        {
            var file = book.GetFileByName("content.opf");

            using (var stream = File.Open(file, FileMode.Open))
            {
                var docContents = XDocument.Load(stream);
                var nameSpaceContents = docContents.Root.Name.Namespace;

                var metadata = docContents.Descendants(nameSpaceContents + "metadata").Descendants();

                var titleNode = metadata.FirstOrDefault(e => e.Name.LocalName == "title");
                var authorNode = metadata.FirstOrDefault(e => e.Name.LocalName == "creator");
                var coverNode = metadata.FirstOrDefault(e => e.Attributes().Any(a => a.Name == "name" && a.Value == "cover"));

                book.Title = titleNode?.Value;
                book.Author = authorNode?.Value;
                book.CoverLocation = coverNode?.Attributes().FirstOrDefault(a => a.Name == "content")?.Value;
            }

            return book;
        }

        public static List<ChapterNavigationItem> GetChapterList(this Book book)
        {
            var file = book.GetFileByName("toc.ncx");

            using (var stream = File.Open(file, FileMode.Open))
            {
                var docTableOfContents = XDocument.Load(stream);
                var nameSpaceTableOfContents = docTableOfContents.Root.Name.Namespace;
                var tableOfContents = docTableOfContents.Descendants(nameSpaceTableOfContents + "navMap").FirstOrDefault();

                return FindChapterList(tableOfContents);
            }
        }

        public static async Task<Bitmap> GetCoverAsync(this Book book)
        {
            if (string.IsNullOrEmpty(book.CoverLocation))
                return null;

            var file = book.GetFileByName($"{book.CoverLocation}*");

            if (file == null)
                return null;

            try
            {
                using (var stream = File.Open(file, FileMode.Open))
                {
                    return await BitmapFactory.DecodeStreamAsync(stream);
                }
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public static StreamReader GetDataSteamReader(this Book book, string path)
        {
            var src = path.Split("/").Last();

            var file = book.GetFileByName(src);
            var stream = File.Open(file, FileMode.Open);
            return new StreamReader(stream);
        }

        public static string GetChapterUrl(this Book book, string path)
        {
            var src = path.Split("/").Last();
            var url = book.GetFileByName(src);

            return $"file:///{url}";
        }

        public static void RemoveData(this Book book)
        {
            var directory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), book.BookId);
            Directory.Delete(directory, true);
        }

        private static string GetFileByName(this Book book, string filenamePattern)
        {
            var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), book.BookId);
            return Directory.GetFiles(path, filenamePattern, SearchOption.AllDirectories).FirstOrDefault();
        }

        private static List<ChapterNavigationItem> FindChapterList(XElement root)
        {
            var elements = root.Elements().Where(d => d.Name.LocalName == "navPoint");
            var chapterList = new List<ChapterNavigationItem>();

            foreach (var element in elements)
            {
                var chapterElements = element.Elements();

                var chapterElement = chapterElements.FirstOrDefault(d => d.Name.LocalName == "navLabel");
                var chapterTitle = chapterElement.Elements().FirstOrDefault(d => d.Name.LocalName == "text")?.Value;

                var chapterSrcElement = chapterElements.FirstOrDefault(d => d.Name.LocalName == "content");
                var chapterSrc = chapterSrcElement.Attributes().FirstOrDefault(a => a.Name == "src")?.Value;

                var chapter = new ChapterNavigationItem
                {
                    Text = chapterTitle,
                    Src = chapterSrc,
                    SubChapters = FindChapterList(element)
                };

                chapterList.Add(chapter);
            }

            return chapterList;
        }
    }
}
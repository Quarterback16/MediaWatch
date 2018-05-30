using System;
using System.Xml;

namespace MediaWatch.Models
{
    public class MediaItem
    {
        private XmlNode node;

        public MediaItem()
        {
        }

        public MediaItem(XmlNode node)
        {
            // TODO: Complete member initialization
            this.node = node;
            foreach (XmlNode n in node.ChildNodes)
            {
                switch (n.Name)
                {
                    case "filename":
                        Filename = n.InnerText;
                        break;

                    case "type":
                        Type = n.InnerText;
                        break;

                    case "title":
                        Title = n.InnerText;
                        break;

                    case "episode":
                        Episode = n.InnerText;
                        break;

                    case "libdate":
                        LibraryDate = DateTime.Parse(n.InnerText);
                        break;

                    default:
                        break;
                }
            }
        }

        public string Filename { get; set; }  //  this is the xml key
        public string Title { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
        public string Episode { get; set; }

        public DateTime LibraryDate { get; set; }

        public override string ToString()
        {
            return Filename;
        }
    }
}
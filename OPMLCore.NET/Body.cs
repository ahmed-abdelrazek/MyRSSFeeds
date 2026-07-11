using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

// from https://github.com/fnya/OPMLCore.NET with MIT License 2018

namespace OPMLCore.NET
{
    public class Body
    {
        ///<summary>
        /// Outline list
        ///</summary>
        public List<Outline> Outlines { get; set; } = new List<Outline>();

        ///<summary>
        /// Constructor
        ///</summary>
        public Body()
        {

        }

        public Body(List<Outline> outlines)
        {
            Outlines = outlines;
        }

        ///<summary>
        /// Constructor
        ///</summary>
        /// <param name="element">element of Body</param>
        public Body(XmlElement element)
        {
            if (element.Name.Equals("body", StringComparison.CurrentCultureIgnoreCase))
            {
                foreach (XmlNode node in element.ChildNodes)
                {
                    if (node.Name.Equals("outline", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Outlines.Add(new Outline((XmlElement)node));
                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("<body>\r\n");
            foreach (Outline outline in Outlines)
            {
                buf.Append(outline.ToString());
            }
            buf.Append("</body>\r\n");

            return buf.ToString();
        }
    }
}

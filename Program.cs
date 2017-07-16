using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication8
{
    class HTMLTagHighlighting
    {
        static void Main(string[] args)
        {
            //Console.WriteLine(HTMLReader.GetWebPageHTML("http://rtai.co.uk/blog/post/3/Building-a-REST-API-in-Golang-With-a-SQL-ORM-and-JWT-Authentication"));

            /*foreach (HTMLTag tag in HTMLReader.GetHTMLTags(HTMLReader.GetWebPageHTML("http://rtai.co.uk/blog/post/3/Building-a-REST-API-in-Golang-With-a-SQL-ORM-and-JWT-Authentication")))
            {
                Console.WriteLine("Start Pos: {0}\nEnd Pos: {1}\nOpening Tag: {2}\nTag Text: {3}\n\n\n\n", tag.StartPos, tag.EndPos, tag.OpeningTag, tag.TagText);
            }*/

            /*foreach (HTMLTagPair tagPair in HTMLReader.PairHTMLTags(HTMLReader.GetHTMLTags(HTMLReader.GetWebPageHTML("http://rtai.co.uk/blog/post/3/Building-a-REST-API-in-Golang-With-a-SQL-ORM-and-JWT-Authentication"))))
            {
                bool isPair = (!tagPair.isCloseOnly && !tagPair.isOpenOnly);
                Console.WriteLine("Open Start Pos: {0}\nOpen End Pos: {1}\nClose Start Pos: {2}\nClose End Pos: {3}\nIs Pair:{4}\n\n\n\n", tagPair.OpenStartPos, tagPair.OpenEndPos,
                    tagPair.ClosingStartPos, tagPair.ClosingEndPos, isPair);
            }*/

            string htmlText = HTMLReader.GetWebPageHTML("http://rtai.co.uk/blog/post/3/Building-a-REST-API-in-Golang-With-a-SQL-ORM-and-JWT-Authentication");
            List<HTMLTagPair> TagPairs = HTMLReader.PairHTMLTags(HTMLReader.GetHTMLTags(htmlText));

            List<int> pairTagIdx = new List<int>();
            List<int> openOnlyTagIdx = new List<int>();
            List<int> closeOnlyTagIdx = new List<int>();

            /// <summary>
            /// Example usage:
            /// Gets the positions of open/closing pairs, followed by non-paired tags.
            /// </summary>
            foreach (HTMLTagPair _tagPair in TagPairs)
            {
                if (!_tagPair.isOpenOnly && !_tagPair.isCloseOnly)
                {
                    for (int i = _tagPair.OpenStartPos; i < _tagPair.OpenEndPos; ++i)
                    {
                        pairTagIdx.Add(i);
                    }
                    for (int j = _tagPair.ClosingStartPos; j < _tagPair.ClosingEndPos; ++j)
                    {
                        pairTagIdx.Add(j);
                    }
                }
                else if (_tagPair.isOpenOnly)
                {
                    for (int i = _tagPair.OpenStartPos; i < _tagPair.OpenEndPos; ++i)
                    {
                        openOnlyTagIdx.Add(i);
                    }
                }
                else if (_tagPair.isCloseOnly)
                {
                    for (int i = _tagPair.OpenStartPos; i < _tagPair.OpenEndPos; ++i)
                    {
                        closeOnlyTagIdx.Add(i);
                    }
                }
            }

            /// <summary>
            /// Example usage:
            /// Colours the console based upon char position existing within a paired, unpaired, or non-tagged area.
            /// </summary>
            for (int i = 0; i < htmlText.Length; ++i)
            {
                if (pairTagIdx.Contains(i))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else if (openOnlyTagIdx.Contains(i))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if (closeOnlyTagIdx.Contains(i))
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.Write(htmlText[i]);
            }


            while (true) ;
        }

        public static class HTMLReader
        {
            // For example only, simple web request/stream reader to get some html.
            public static string GetWebPageHTML(string url)
            {
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(url);
                try
                {
                    using (HttpWebResponse Response = (HttpWebResponse)Request.GetResponse())
                    {
                        if (Response.StatusCode == HttpStatusCode.OK)
                        {
                            Stream receiveStream = Response.GetResponseStream();
                            StreamReader readStream = null;

                            if (Response.CharacterSet == null)
                            {
                                readStream = new StreamReader(receiveStream);
                            }
                            else
                            {
                                readStream = new StreamReader(receiveStream, Encoding.GetEncoding(Response.CharacterSet));
                            }
                            return readStream.ReadToEnd();
                        }
                        else
                        {
                            return Response.StatusCode.ToString() + " unable to retrieve data from page.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }

            /// <summary>
            /// Takes in a string containing html text data and returns a list of the html tags contained within it.
            /// </summary>
            /// <param name="HTMLData">String containing html text.</param>
            /// <returns>Returns the html tags contained within passed in html data.</returns>
            public static List<HTMLTag> GetHTMLTags(string HTMLData)
            {
                List<HTMLTag> HTMLTags = new List<HTMLTag>();

                for (int i = HTMLData.IndexOf('<'); i > -1; i = HTMLData.IndexOf('<', i + 1))
                {
                    int endPos = HTMLData.IndexOf('>', i + 1);
                    // for loop end when i=-1 ('<' not found)
                    HTMLTag tag = new HTMLTag()
                    {
                        StartPos = i,
                        // if no /, assume opening tag
                        OpeningTag = HTMLData[i + 1] != '/',
                        EndPos = endPos,
                        // Don't include < or > (by offsetting i/index), if data includes a closing tag '/', then offset i further.
                        TagText = HTMLData[i + 1] != '/' ? HTMLData.Substring(i + 1, (endPos - (i + 1))) : HTMLData.Substring(i + 2, (endPos - (i + 2)))
                    };
                    if (tag.TagText.Contains(" "))
                    {
                        tag.TagText = tag.TagText.Substring(0, tag.TagText.IndexOf(' '));
                    }
                    // Could extend to add mark-up tags to their own list.
                    if (!(tag.TagText.Contains("!") || tag.TagText.Contains("-")))
                    {
                        HTMLTags.Add(tag);
                    }
                }
                return HTMLTags;
            }
            /// <summary>
            /// Takes in a list of HTMLTag objects, merges them into a paired tag object based upon the value of their position parameters.
            /// </summary>
            /// <param name="HTMLTags">List of HTML Tags</param>
            /// <returns>List of Paired HTML Tags</returns>
            public static List<HTMLTagPair> PairHTMLTags(List<HTMLTag> HTMLTags)
            {
                // Create tag pair list
                List<HTMLTagPair> TagPairs = new List<HTMLTagPair>();
                // loop through tags
                for (int i = HTMLTags.Count - 1; i > -1; --i)
                {
                    HTMLTag openTag = new HTMLTag();
                    HTMLTag closeTag = new HTMLTag();
                    bool paired = false;

                    // if we found an opening tag
                    if (HTMLTags[i].OpeningTag)
                    {
                        openTag = HTMLTags[i];
                        // loop through our list again (starting at idx i)
                        for (int j = 0; j < HTMLTags.Count; ++j)
                        {
                            // if we found a closing tag
                            if (!HTMLTags[j].OpeningTag)
                            {
                                // if our tag text matches and start position is lower
                                if (string.Equals(HTMLTags[i].TagText, HTMLTags[j].TagText, StringComparison.InvariantCultureIgnoreCase)
                                    && (HTMLTags[i].StartPos < HTMLTags[j].StartPos))
                                {
                                    // if we already have a match
                                    if (paired)
                                    {
                                        // check if this match is closer
                                        if ((HTMLTags[j].StartPos - HTMLTags[i].StartPos) < (openTag.StartPos - closeTag.StartPos))
                                        {
                                            closeTag = HTMLTags[j];
                                        }
                                    }
                                    else
                                    {
                                        // no match, this matching tag is closest by default
                                        closeTag = HTMLTags[j];
                                        paired = true;
                                    }
                                }
                            }
                        }
                        // if we found a match during the ?bubble sort.
                        if (paired)
                        {
                            HTMLTagPair tagPair = new HTMLTagPair()
                            {
                                OpenStartPos = openTag.StartPos,
                                OpenEndPos = openTag.EndPos,
                                ClosingStartPos = closeTag.StartPos,
                                ClosingEndPos = closeTag.EndPos,
                                isOpenOnly = false,
                                isCloseOnly = false
                            };
                            TagPairs.Add(tagPair);
                            // Remove tags from list to prevent re-matching
                            HTMLTags.Remove(openTag);
                            HTMLTags.Remove(closeTag);
                        }
                        else
                        {
                            // if no match, add our open tag and set the isOpenOnly flag (could not find a matching closing html tag).
                            HTMLTagPair tagPair = new HTMLTagPair()
                            {
                                OpenStartPos = openTag.StartPos,
                                OpenEndPos = openTag.EndPos,
                                ClosingStartPos = -1,
                                ClosingEndPos = -1,
                                isOpenOnly = true,
                                isCloseOnly = false
                            };
                            TagPairs.Add(tagPair);
                            // remove the open tag from our list
                            HTMLTags.Remove(openTag);
                        }
                    }
                }
                // Whatever is left in our list can be copied across as an unmatched close tag. Implicit conversion might look a bit cleaner?
                if (HTMLTags.Count > 0)
                {
                    foreach (HTMLTag tag in HTMLTags)
                    {
                        TagPairs.Add(new HTMLTagPair()
                        {
                            ClosingStartPos = tag.StartPos,
                            ClosingEndPos = tag.EndPos,
                            OpenStartPos = -1,
                            OpenEndPos = -1,
                            isCloseOnly = true,
                            isOpenOnly = false,
                        });
                    }
                }
                return TagPairs;
            }
        }

        public class HTMLTagPair
        {
            public int OpenStartPos { get; set; }
            public int ClosingStartPos { get; set; }
            public int OpenEndPos { get; set; }
            public int ClosingEndPos { get; set; }
            public bool isOpenOnly { get; set; }
            public bool isCloseOnly { get; set; }

            public HTMLTagPair() { }
        }

        public class HTMLTag
        {
            public int StartPos { get; set; }
            public int EndPos { get; set; }
            public bool OpeningTag { get; set; }
            public string TagText { get; set; }

            public HTMLTag() { }
        }

    }
}

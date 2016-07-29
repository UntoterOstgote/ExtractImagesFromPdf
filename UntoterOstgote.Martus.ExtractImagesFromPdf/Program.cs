using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UntoterOstgote.Martus.ExtractImagesFromPdf
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ExtractImagesFromPdf <InputDirectory>");
            }
            else
            {
                DirectoryInfo inputDir = new DirectoryInfo(args[0]);

                foreach (var file in inputDir.GetFiles("*.pdf"))
                {
                    Console.WriteLine(DateTime.Now.ToString() + ": Processing File " + file.Name);

                    PdfReader reader = new PdfReader(file.FullName);
                    PdfReaderContentParser parser = new PdfReaderContentParser(reader);
                    PdfImageRenderListener listener = new PdfImageRenderListener();
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        parser.ProcessContent(i, listener);
                        if (i % 10 == 0)
                        {
                            Console.WriteLine(DateTime.Now.ToString() + ": Processing Page " + i);
                        }
                    }
                    for (int i = 0; i < listener.Images.Count; ++i)
                    {
                        DirectoryInfo outputDir = inputDir.CreateSubdirectory(file.Name.Substring(0, file.Name.Length -4));
                        string path = System.IO.Path.Combine(outputDir.FullName, listener.ImageNames[i]);
                        using (FileStream fs = new FileStream(
                          path, FileMode.Create, FileAccess.Write
                        ))
                        {
                            fs.Write(listener.Images[i], 0, listener.Images[i].Length);
                        }

                        if (i % 10 == 0)
                        {
                            Console.WriteLine(DateTime.Now.ToString() + ": Extracting Image " + i);
                        } 
                    }
                }
            }       
        }

        public class PdfImageRenderListener : IRenderListener
        {
            public void RenderText(TextRenderInfo renderInfo) { }
            public void BeginTextBlock() { }
            public void EndTextBlock() { }

            public List<byte[]> Images = new List<byte[]>();
            public List<string> ImageNames = new List<string>();
            public void RenderImage(ImageRenderInfo renderInfo)
            {
                PdfImageObject image = renderInfo.GetImage();
                try
                {
                    image = renderInfo.GetImage();
                    if (image == null) return;

                    ImageNames.Add(string.Format(
                      "Image{0}.{1}", renderInfo.GetRef().Number, image.GetFileType()
                    ));
                    using (MemoryStream ms = new MemoryStream(image.GetImageAsBytes()))
                    {
                        Images.Add(ms.ToArray());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
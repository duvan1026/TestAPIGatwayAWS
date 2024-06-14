using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using Slyg.Tools.Imaging;
using Newtonsoft.Json.Linq;
using TestAPIGatewayAWS.Models;
using static TestAPIGatewayAWS.Program;
using System.Linq;
using TestAPIGatewayAWS.Models.RelationShips;
using TestAPIGatewayAWS.Models.Block;
using TestAPIGatewayAWS.OCR;


namespace TestAPIGatewayAWS
{
    internal class Program
    {
        //static string pathbase = @"C:\Program Files (x86)\PYC\OCR_Results\Out_Data\33323411";
        //static string pathbase = @"C:\Users\duvan.castro\Desktop\exportacion\images\0000080954\3613856111";
        //static int _ImageWidth = 1701;
        //static int _ImageHeigth = 2216;

        static string pathbase = @"C:\Users\duvan.castro\Desktop\exportacion\images\0000080954\3613856411";
        static int _ImageWidth = 1719;
        static int _ImageHeigth = 2639;

        public class DataStructureImageBytes
        {
            public byte[] ImageBytes { get; set; }
        }
        public class ImagePayload
        {
            public string ImageBytes { get; set; }
        }

        static byte[] ConvertImageToTIFFBytes(string rutaImagen)
        {
            // Carga la imagen desde la ruta especificada
            using (Image image = Image.FromFile(rutaImagen))
            {
                // Crea un MemoryStream para almacenar la imagen en formato TIFF.
                using (MemoryStream ms = new MemoryStream())
                {
                    // Obtiene el codificador de formato TIFF.
                    ImageCodecInfo tiffEncoder = GetEncoderInfo("image/tiff");

                    // Crea los parámetros del codificador para especificar la compresión (en este caso, sin compresión).
                    EncoderParameters encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionNone);

                    // Guarda la imagen en el MemoryStream en formato TIFF sin compresión.
                    image.Save(ms, tiffEncoder, encoderParams);

                    // Convierte el contenido del MemoryStream a un array de bytes y lo devuelve.
                    return ms.ToArray();
                }
            }
        }

        static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Obtener la lista de codificadores de imágenes disponibles
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

            // Iterar a través de los codificadores disponibles
            foreach (ImageCodecInfo encoder in encoders)
            {
                // Comprobar si el tipo MIME del codificador coincide con el tipo MIME especificado
                if (encoder.MimeType == mimeType)
                {
                    // Devolver el codificador correspondiente
                    return encoder;
                }
            }
            return null;
        }

        public static byte[] ConvertImageBytesToGrayscaleBytes(byte[] imageBytes)
        {
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                using (Bitmap originalBitmap = new Bitmap(ms))
                {
                    // Crear un nuevo bitmap en escala de grises
                    Bitmap grayBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height, PixelFormat.Format8bppIndexed);

                    // Set the grayscale palette
                    ColorPalette palette = grayBitmap.Palette;
                    for (int i = 0; i < 256; i++)
                    {
                        palette.Entries[i] = Color.FromArgb(i, i, i);
                    }
                    grayBitmap.Palette = palette;

                    // Bloquear los bits para un acceso rápido a los píxeles
                    BitmapData originalData = originalBitmap.LockBits(new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height), ImageLockMode.ReadOnly, originalBitmap.PixelFormat);
                    BitmapData grayData = grayBitmap.LockBits(new Rectangle(0, 0, grayBitmap.Width, grayBitmap.Height), ImageLockMode.WriteOnly, grayBitmap.PixelFormat);

                    int bytesPerPixel = Image.GetPixelFormatSize(originalBitmap.PixelFormat) / 8;
                    int height = originalBitmap.Height;
                    int width = originalBitmap.Width;

                    unsafe
                    {
                        byte* originalScan0 = (byte*)originalData.Scan0.ToPointer();
                        byte* grayScan0 = (byte*)grayData.Scan0.ToPointer();

                        for (int y = 0; y < height; y++)
                        {
                            byte* originalRow = originalScan0 + (y * originalData.Stride);
                            byte* grayRow = grayScan0 + (y * grayData.Stride);

                            for (int x = 0; x < width; x++)
                            {
                                byte blue = originalRow[x * bytesPerPixel];
                                byte green = originalRow[x * bytesPerPixel + 1];
                                byte red = originalRow[x * bytesPerPixel + 2];

                                // Calcular el valor en escala de grises
                                byte grayValue = (byte)(red * 0.3 + green * 0.59 + blue * 0.11);
                                grayRow[x] = grayValue;
                            }
                        }
                    }

                    // Desbloquear los bits
                    originalBitmap.UnlockBits(originalData);
                    grayBitmap.UnlockBits(grayData);

                    // Convertir la imagen a un arreglo de bytes
                    using (MemoryStream grayMs = new MemoryStream())
                    {
                        grayBitmap.Save(grayMs, ImageFormat.Tiff);
                        return grayMs.ToArray();
                    }
                }
            }
        }

        public static string SerializeImageBytes(byte[] imageBytes)
        {
            var imageBytesData = new DataStructureImageBytes
            {
                ImageBytes = imageBytes
            };

            return JsonConvert.SerializeObject(imageBytesData);
        }


        public static string SendImageAsync(string imagePath, string url)
        {
            byte[] bytesTIFF = ConvertImageToTIFFBytes(imagePath);
            byte[] grayscaleBytes = ConvertImageBytesToGrayscaleBytes(bytesTIFF);

            string json = SerializeImageBytes(grayscaleBytes);

            string filePath = Program.pathbase + "\\Json_292046111.txt";
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(json);
            }

            return SendDataPostRequest(json, url);
        }

        public static string SendDataPostRequest(string jsonPayload, string url)
        {
            try
            {
                // Crea la solicitud HTTP con la URL especificada
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";

                // Agrega la cabecera Accept-Encoding para indicar la aceptación de compresión
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");

                // Escribe el JSON en el cuerpo de la solicitud
                using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(jsonPayload);
                }

                // Obtiene y devuelve la respuesta
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }

                // Si no se pudo obtener una respuesta, se devuelve una cadena vacía
                return string.Empty;
            }
            catch (Exception ex)
            {
                 throw;
            }

        }










        static void Main(string[] args)
        {
            /* ///////////////////////// Codigo para enxtraer texto de imagenes con AWS ///////////////////////////
              
            //string imagePath = @"C:\Program Files (x86)\PYC\OCR_Results\Out_Data\33323411\333234111.tif";
            //string filePath = @"C:\Program Files (x86)\PYC\OCR_Results\Out_Data\33323411\HOCR_292046111.txt";
            //string imagePath = Program.pathbase + "\\333234111.tif";
            string nameImage = "36138564111";
            string imagePath = Program.pathbase + "\\"+ nameImage +".tif";

            string filePath = Program.pathbase + "\\HOCR_"+ nameImage + ".txt";


            string url = "https://ylbktmueei.execute-api.us-east-2.amazonaws.com/ExtractOCR";
            string result = SendImageAsync(imagePath, url);

            if (result != null)
            {
                // Convertir el string JSON a un objeto JSON
                var jsonObject = JsonConvert.DeserializeObject(result);
                var formattedJson = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);

                // Usar StreamWriter para escribir el contenido en el archivo
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.Write(formattedJson);
                }

                // Procesar el JSON para extraer las líneas de texto
                var jsonArray = JArray.Parse(result);
                var lines = new List<string>();

                foreach (var item in jsonArray)
                {
                    if (item["BlockType"] != null && item["BlockType"].ToString() == "LINE")
                    {
                        var text = item["Text"]?.ToString();
                        if (text != null)
                        {
                            lines.Add(text);
                        }
                    }
                }

                // Unir las líneas en un solo string, cada una separada por un salto de línea
                string allLines = string.Join(Environment.NewLine, lines);

                string textFilePath = Program.pathbase + "\\LinesDetect_"  + nameImage + ".txt"; // Archivo para guardar las líneas de texto

                // Escribir las líneas en un archivo de texto
                using (StreamWriter writer = new StreamWriter(textFilePath))
                {
                    writer.Write(allLines);
                }
            }
            *////////////////////////////////////////////////////

            
            //string nameImage = "36138561111";
            string nameImage = "36138564111";
            
            string imagePath = Program.pathbase + "\\" + nameImage + ".tif";
            string filePath = Program.pathbase + "\\HOCR_" + nameImage + ".txt";
            string filePathHtmlOCRAWS = Program.pathbase + "\\HOCR_AWS_" + nameImage + ".txt";
            string imageGraphicslines = Program.pathbase + "\\imageGraphicslines" + nameImage + ".tif";
            string imageGraphicsParagraph = Program.pathbase + "\\imageGraphicsParagraph" + nameImage + ".tif";


            string jsonString = File.ReadAllText(filePath);                         // Read the JSON text file into a string
            OCRProcessAWS processAWSOCR = new OCRProcessAWS(_ImageWidth, _ImageHeigth);
            string htmlOCRAWS = processAWSOCR.GenerateHtmlOCRTesseractforJsonAWS(jsonString);

            /*
            List<Block> dataList = JsonConvert.DeserializeObject<List<Block>>(jsonString);                           // Deserialize the JSON string into a JObject
            
            BlockPage dataExtractOCRAWS = GetExtractedStructureFromOcr(dataList);
            var LinesOCR = dataExtractOCRAWS.RelationshipsLines.Lines;

            // Ordenar por eje Y (Top) y luego por eje X (Left)
            var orderedLinesOCR = LinesOCR
                .OrderBy(line => line.Geometry.BoundingBox.Top)
                .ThenBy(line => line.Geometry.BoundingBox.Left)
                .ToList();

            var paragraphs = DetectParagraphs(orderedLinesOCR, 0.5);                   //Detecta los parrafos que contiene la pagina
           
            BlockPageParagraph pageParagraphs = new BlockPageParagraph();
            pageParagraphs.BlockParagraph = paragraphs;
            pageParagraphs.Id = "page_1";
            pageParagraphs.Id = "ocr_page";
            pageParagraphs.Geometry = dataExtractOCRAWS.Geometry;

            string htmlOCRAWS = GenerateHtml(pageParagraphs);
            */

            ///////////////////////////////  EXTRAS   ////////////////////////////////////////////
            using (StreamWriter writer = new StreamWriter(filePathHtmlOCRAWS))
            {
                writer.Write(htmlOCRAWS);
            }

            //GraphicsBoundingBoxLines(imagePath, orderedLinesOCR, imageGraphicslines);
            //GraphicsBoundingBoxParagraphs(imagePath, paragraphs, imageGraphicsParagraph);
            
        }


        private static void GraphicsBoundingBoxLines(string imagePath, List<BlockLine> orderedLinesOCR, string imageGraphicsParagraph)
        {
            // Cargar la imagen original
            using (Image originalImage = Image.FromFile(imagePath))
            {
                // Crear un nuevo bitmap basado en la imagen original
                using (Bitmap bitmap = new Bitmap(originalImage))
                {
                    // Crear un objeto Graphics para dibujar en el bitmap
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        // Definir el color y el grosor del recuadro
                        Pen pen = new Pen(Color.Blue, 2);

                        // Dibujar los recuadros en el bitmap
                        foreach (var line in orderedLinesOCR)
                        {
                            var boundingBox = line.Geometry.BoundingBox;

                            // Calcular las coordenadas y dimensiones del recuadro en píxeles
                            int x = (int)(boundingBox.Left);
                            int y = (int)(boundingBox.Top);
                            int width = (int)(boundingBox.Width);
                            int height = (int)(boundingBox.Height);

                            // Dibujar el recuadro
                            graphics.DrawRectangle(pen, x, y, width, height);
                        }
                    }

                    // Guardar la nueva imagen en la ruta especificada
                    bitmap.Save(imageGraphicsParagraph, ImageFormat.Tiff);
                }
            }
        }

        private static void GraphicsBoundingBoxParagraphs(string imagePath, List<BlockParagraph> paragraphs, string imageGraphicsParagraph)
        {
            // Cargar la imagen original
            using (Image originalImage = Image.FromFile(imagePath))
            {
                // Crear un nuevo bitmap basado en la imagen original
                using (Bitmap bitmap = new Bitmap(originalImage))
                {
                    // Crear un objeto Graphics para dibujar en el bitmap
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        // Definir el color y el grosor del recuadro
                        Pen pen = new Pen(Color.Blue, 2);

                        // Dibujar los recuadros en el bitmap
                        foreach (var paragraph in paragraphs)
                        {
                            var boundingBox = paragraph.Geometry.BoundingBox;

                            // Calcular las coordenadas y dimensiones del recuadro en píxeles
                            int x = (int)(boundingBox.Left);
                            int y = (int)(boundingBox.Top);
                            int width = (int)(boundingBox.Width);
                            int height = (int)(boundingBox.Height);

                            // Dibujar el recuadro
                            graphics.DrawRectangle(pen, x, y, width, height);
                        }
                    }

                    // Guardar la nueva imagen en la ruta especificada
                    bitmap.Save(imageGraphicsParagraph, ImageFormat.Tiff);
                }
            }
        }

    }
}

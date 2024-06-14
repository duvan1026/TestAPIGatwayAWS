using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAPIGatewayAWS.Models.Block;
using TestAPIGatewayAWS.Models.RelationShips;
using TestAPIGatewayAWS.Models;
using Newtonsoft.Json;

namespace TestAPIGatewayAWS.OCR
{
    public class OCRProcessAWS
    {
        private int _ImageWidth;
        private int _ImageHeight;

        public OCRProcessAWS(int imageWidth, int imageHeight) 
        {
            _ImageWidth = imageWidth;
            _ImageHeight = imageHeight;
        }

        /// <summary>
        /// Obtiene una lista de palabras identificadas mediante OCR a partir de una lista de IDs de palabras y bloques de texto.
        /// </summary>
        /// <param name="_listIdsWords">Lista de IDs de palabras a buscar.</param>
        /// <param name="_WordsBlocks">Lista de bloques de texto donde buscar las palabras.</param>
        /// <returns>Una lista de objetos BlockWord que representan las palabras identificadas.</returns>
        private List<BlockWord> GetWordsReadOCR(List<string> _listIdsWords, List<Block> _WordsBlocks)
        {
            try
            {
                List<BlockWord> words = new List<BlockWord>();

                foreach (var idsWord in _listIdsWords)
                {
                    List<Block> identityLinesBlocks = _WordsBlocks.Where(block => block.Id == idsWord).ToList();

                    if (identityLinesBlocks != null && identityLinesBlocks.Count > 0)
                    {
                        BlockWord blockWord = new BlockWord();
                        blockWord.Confidence = identityLinesBlocks[0].Confidence;
                        blockWord.Text = identityLinesBlocks[0].Text;
                        blockWord.TextType = identityLinesBlocks[0].TextType;
                        blockWord.Id = identityLinesBlocks[0].Id;

                        Geometry geometry = Geometry.CopyAndRescaleGeometry(identityLinesBlocks[0].Geometry, _ImageWidth, _ImageHeight);
                        blockWord.Geometry = geometry;

                        words.Add(blockWord);
                    }
                }

                return words;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Obtiene una lista de líneas extraídas mediante OCR a partir de una lista de IDs de líneas, bloques de líneas y bloques de palabras.
        /// </summary>
        /// <param name="_listIdsLines">Lista de IDs de líneas a buscar.</param>
        /// <param name="_LinesBlocks">Lista de bloques de líneas donde buscar las líneas.</param>
        /// <param name="_WordsBlocks">Lista de bloques de palabras relacionados para cada línea encontrada.</param>
        /// <returns>Una lista de objetos BlockLine que representan las líneas extraídas.</returns>
        private List<BlockLine> GetExtractedLinesReadOCR(List<string> _listIdsLines, List<Block> _LinesBlocks, List<Block> _WordsBlocks)
        {
            List<BlockLine> lines = new List<BlockLine>();

            try
            {
                foreach (var idsLine in _listIdsLines)
                {
                    List<Block> identityLinesBlocks = _LinesBlocks.Where(block => block.Id == idsLine).ToList();

                    if (identityLinesBlocks != null && identityLinesBlocks.Count > 0)
                    {
                        BlockLine blockLines = new BlockLine();
                        blockLines.Confidence = identityLinesBlocks[0].Confidence;
                        blockLines.Text = identityLinesBlocks[0].Text;
                        blockLines.Id = identityLinesBlocks[0].Id;

                        Geometry geometry = Geometry.CopyAndRescaleGeometry(identityLinesBlocks[0].Geometry, _ImageWidth, _ImageHeight);
                        blockLines.Geometry = geometry;

                        blockLines.RelationshipsWords = new RelationshipsWords();
                        blockLines.RelationshipsWords.Type = identityLinesBlocks[0].Relationships[0].Type;

                        List<string> listIdsWords = identityLinesBlocks[0].Relationships[0].Ids;
                        blockLines.RelationshipsWords.Words = GetWordsReadOCR(listIdsWords, _WordsBlocks);

                        lines.Add(blockLines);
                    }
                }

                return lines;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Extrae y construye una estructura BlockPage a partir de datos OCR Json AWS.
        /// </summary>
        /// <param name="_JsonBlocksOCR">Una lista de bloques obtenidos del OCR, que contiene tipos PAGE, LINE y WORD.</param>
        /// <returns>
        /// Un objeto BlockPage que contiene la geometría y las relaciones del primer bloque PAGE, con sus respectivas lineas y palabras que lo componen,
        /// o null si no se encuentra ningún bloque PAGE.
        /// </returns>
        private  BlockPage GetExtractedStructureFromOcr(List<Block> _JsonBlocksOCR)
        {
            try
            {
                List<Block> pageBlocks = _JsonBlocksOCR.Where(block => block.BlockType == "PAGE").ToList();
                if (pageBlocks == null) return null; // No encuentra información de la pagina

                List<Block> linesBlocks = _JsonBlocksOCR.Where(block => block.BlockType == "LINE").ToList();
                List<Block> wordsBlocks = _JsonBlocksOCR.Where(block => block.BlockType == "WORD").ToList();


                Geometry geometry = Geometry.CopyAndRescaleGeometry(pageBlocks[0].Geometry, _ImageWidth, _ImageHeight);
                var relationshipsLines = new RelationshipsLines
                {
                    Type = pageBlocks[0].Relationships[0].Type,
                    Lines = GetExtractedLinesReadOCR(pageBlocks[0].Relationships[0].Ids, linesBlocks, wordsBlocks)
                };

                return new BlockPage
                {
                    Geometry = geometry,
                    Id = pageBlocks[0].Id,
                    RelationshipsLines = relationshipsLines
                };
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Calcula la altura promedio de las líneas en un párrafo dado.
        /// Si el párrafo contiene más de una línea, calcula el promedio de las alturas de todas las líneas.
        /// Si el párrafo contiene una sola línea, devuelve la altura de esa línea.
        /// </summary>
        /// <param name="currentParagraph">El párrafo actual con las líneas relacionadas.</param>
        /// <returns>La altura promedio de las líneas del párrafo.</returns>
        public double CalculateMeanLineHeight(BlockParagraph currentParagraph)
        {
            double meanLineHeightCurrentParagraph;

            try
            {
                // Extrae el valor del porcentaje de altura de la línea anterior 
                if (currentParagraph.RelationshipsLines.Lines.Count > 1)
                {
                    int lineCount = 0;                              // Variable para contar el número de líneas
                    double totalHeight = 0;                         // Variable para almacenar la suma de alturas

                    foreach (var lineParagraph in currentParagraph.RelationshipsLines.Lines)
                    {
                        totalHeight += lineParagraph.Geometry.BoundingBox.Height;
                        lineCount++;
                    }

                    meanLineHeightCurrentParagraph = totalHeight / lineCount;
                }
                else
                {
                    meanLineHeightCurrentParagraph = currentParagraph.RelationshipsLines.Lines.Last().Geometry.BoundingBox.Height;
                }

                return meanLineHeightCurrentParagraph;
            }
            catch
            {
                throw;
            }
        }



        /// <summary>
        /// Método para detectar párrafos a partir de una lista de líneas
        /// </summary>
        /// <param name="orderedLinesOCR"></param>
        /// <param name="maxVerticalGapPercentage"></param>
        /// <returns></returns>
        public  List<BlockParagraph> DetectParagraphs(List<BlockLine> orderedLinesOCR, double maxVerticalGapPercentage)
        {
            try
            {
                List<BlockParagraph> paragraphs = new List<BlockParagraph>();
                int countParagraph = 1;
                string nameClassParagraph = "ocr_par";

                // Inicializar el primer párrafo y sus propiedades
                BlockParagraph currentParagraph = new BlockParagraph();
                currentParagraph.Id = $"par_1_{countParagraph}";
                currentParagraph.Class = nameClassParagraph;
                currentParagraph.Geometry = orderedLinesOCR[0].Geometry;
                currentParagraph.RelationshipsLines.Lines.Add(orderedLinesOCR[0]);
                paragraphs.Add(currentParagraph);

                // Recorrer las líneas restantes para detectar párrafos
                for (int i = 1; i < orderedLinesOCR.Count; i++)
                {
                    bool addedToExistingParagraph = false;

                    double meanLineHeightCurrentParagraph = CalculateMeanLineHeight(currentParagraph);
                    double maxVerticalGap = maxVerticalGapPercentage * meanLineHeightCurrentParagraph;      // Calcular el máximo margen vertical como un porcentaje de la altura de la última línea del párrafo actual

                    BlockLine line = orderedLinesOCR[i];

                    // Comparar con las líneas en el párrafo actual
                    foreach (var paragraph in paragraphs)
                    {
                        BlockLine lastLineInParagraph = paragraph.RelationshipsLines.Lines.Last();

                        double valueLastLineTop = lastLineInParagraph.Geometry.BoundingBox.Top;
                        double valueLastLineHeight = lastLineInParagraph.Geometry.BoundingBox.Height;
                        double valueLastLineLeft = lastLineInParagraph.Geometry.BoundingBox.Left;
                        double valueLastLineWidth = lastLineInParagraph.Geometry.BoundingBox.Width;
                        double valueLineTop = line.Geometry.BoundingBox.Top;
                        double valueLineLeft = line.Geometry.BoundingBox.Left;

                        double diferrenceTop = 0.0d;
                        double diferrenceLeft = 0.0d;

                        if (valueLineTop >= valueLastLineTop && valueLineTop <= (valueLastLineTop + valueLastLineHeight))
                        {
                            if (valueLineLeft - (valueLastLineLeft + valueLastLineWidth) > 0)
                            {
                                diferrenceLeft = valueLineLeft - (valueLastLineLeft + valueLastLineWidth);
                            }
                        }
                        else
                        {
                            diferrenceTop = valueLineTop - (valueLastLineTop + valueLastLineHeight);
                        }

                        // Verificar proximidad vertical y horizontal
                        if (Math.Abs(diferrenceTop) <= maxVerticalGap && Math.Abs(diferrenceLeft) <= maxVerticalGap)
                        {
                            int countParagraphLines = paragraph.RelationshipsLines.Lines.Count;
                            Geometry unifiedGeometry = (countParagraphLines > 0) ? Geometry.Unify(paragraph.Geometry, line.Geometry) : line.Geometry;
                            paragraph.Geometry = unifiedGeometry;
                            paragraph.RelationshipsLines.Lines.Add(line);
                            addedToExistingParagraph = true;
                            break;
                        }
                    }

                    // Si no se agregó a ningún párrafo existente, crear uno nuevo
                    if (!addedToExistingParagraph)
                    {
                        countParagraph++;

                        // Inicializar el primer párrafo y sus propiedades
                        currentParagraph = new BlockParagraph();
                        currentParagraph.Class = nameClassParagraph;
                        currentParagraph.Id = $"par_1_{countParagraph}";
                        currentParagraph.Geometry = line.Geometry;
                        currentParagraph.RelationshipsLines.Lines.Add(line);
                        paragraphs.Add(currentParagraph);
                    }
                }

                return paragraphs;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// genera un html similar a la data de salida de Tesseract
        /// </summary>
        /// <param name="pageParagraphs"> data de información de la pagina extraida por OCR</param>
        /// <returns>string con formato html similar al de Tesseract</returns>
        private  string GenerateHtmlOCRByTesseract(BlockPageParagraph pageParagraphs)
        {
            StringBuilder html = new StringBuilder();

            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<title></title>");
            html.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\">");
            html.AppendLine("<meta name=\"ocr-system\" content=\"AWS-Textract\">");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine($"<div class=\"{pageParagraphs.Class}\" id =\"{pageParagraphs.Id}\" title=\"image &quot;unknown&quot;; bbox {((int)pageParagraphs.Geometry.BoundingBox.Left)} {(int)pageParagraphs.Geometry.BoundingBox.Top} {(int)(pageParagraphs.Geometry.BoundingBox.Left + pageParagraphs.Geometry.BoundingBox.Width)} {(int)(pageParagraphs.Geometry.BoundingBox.Top + pageParagraphs.Geometry.BoundingBox.Height)}; ppageno 0; scan_res 96 96\">");

            int blockCount = 1;
            int wordCount = 1;
            foreach (var paragraph in pageParagraphs.BlockParagraph)
            {
                html.AppendLine($"   <div class=\"ocr_carea\" id=\"block_1_{blockCount}\" title=\"bbox {(int)paragraph.Geometry.BoundingBox.Left} {(int)paragraph.Geometry.BoundingBox.Top} {(int)(paragraph.Geometry.BoundingBox.Left + paragraph.Geometry.BoundingBox.Width)} {(int)(paragraph.Geometry.BoundingBox.Top + paragraph.Geometry.BoundingBox.Height)}\">");
                html.AppendLine($"    <p class=\"ocr_par\" id=\"{paragraph.Id}\" class=\"{paragraph.Class}\" title=\"bbox {((int)paragraph.Geometry.BoundingBox.Left)} {(int)paragraph.Geometry.BoundingBox.Top} {(int)(paragraph.Geometry.BoundingBox.Left + paragraph.Geometry.BoundingBox.Width)} {(int)(paragraph.Geometry.BoundingBox.Top + paragraph.Geometry.BoundingBox.Height)}\">");

                int lineCount = 1;
                foreach (var line in paragraph.RelationshipsLines.Lines)
                {
                    html.AppendLine($"     <span class=\"ocr_line\" id=\"line_{blockCount}_{lineCount}\" title=\"bbox {(int)line.Geometry.BoundingBox.Left} {(int)line.Geometry.BoundingBox.Top} {(int)(line.Geometry.BoundingBox.Left + line.Geometry.BoundingBox.Width)} {(int)(line.Geometry.BoundingBox.Top + line.Geometry.BoundingBox.Height)}\">");

                    //int wordCount = 1;
                    foreach (var word in line.RelationshipsWords.Words)
                    {
                        html.AppendLine($"      <span class=\"ocrx_word\" id=\"word_{1}_{wordCount}\" title=\"bbox {(int)word.Geometry.BoundingBox.Left} {(int)word.Geometry.BoundingBox.Top} {(int)(word.Geometry.BoundingBox.Left + word.Geometry.BoundingBox.Width)} {(int)(word.Geometry.BoundingBox.Top + word.Geometry.BoundingBox.Height)}; x_wconf {(int)word.Confidence}\">{word.Text}</span>");
                        wordCount++;
                    }

                    html.AppendLine("     </span>");
                    lineCount++;
                }

                html.AppendLine("    </p>");
                html.AppendLine("   </div>");
                blockCount++;
            }

            html.AppendLine("</div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        /// <summary>
        /// convierte Json de AWS textract en un HTML similar a Tesseract
        /// </summary>
        /// <param name="jsonAWSOCR">Json con data extraido de AWS Textract</param>
        /// <returns>string con estrucutura html similar a la data de resultardo de tesseract</returns>
        public string GenerateHtmlOCRTesseractforJsonAWS(string jsonAWSOCR)
        {
            try
            {
                List<Block> dataList = JsonConvert.DeserializeObject<List<Block>>(jsonAWSOCR);                           // Deserialize the JSON string into a JObject

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

                string HtmlOCRAWS = GenerateHtmlOCRByTesseract(pageParagraphs);

                return HtmlOCRAWS;
            }
            catch
            {
                throw;
            }

        }


    }
}

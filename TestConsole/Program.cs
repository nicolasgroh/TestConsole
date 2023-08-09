using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using CsvHelper.Configuration;
using System.Windows;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new Point(5, 6);

            test.Offset(2, 3);

            Console.WriteLine(test);
        }

        #region Gimeg SalesOrder Parsen
        private static bool ParseGimegSalesOrder(string path, HashSet<string> test)
        {
            using (var stream = new StreamReader(path))
            {
                using (var csvReader = new CsvHelper.CsvReader(stream, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = "," }))
                {
                    var salesOrdersToAdd = csvReader.GetRecords<GimegSalesOrder>().ToList();

                    for (int i = 0; i < salesOrdersToAdd.Count; i++)
                    {
                        var salesOrderToAdd = salesOrdersToAdd[i];

                        if (salesOrderToAdd.VendorReference?.Contains("55005246") == true)
                        {
                            Console.WriteLine(path);
                            return true;
                        }
                        if (salesOrderToAdd.CustomerReference?.Contains("55005246") == true)
                        {
                            Console.WriteLine(path);
                            return true;
                        }

                        if (salesOrderToAdd.CustomerReference != null && salesOrderToAdd.CustomerReference.Length >= 3) test.Add(salesOrderToAdd.CustomerReference.Substring(0, 3));
                    }
                }
            }

            return false;
        }
        #endregion

        #region Datenbankbackups umbenennen
        private static void RenameDatabaseBackupFiles()
        {
            var path = @"C:\SQL\Archiv";

            var files = Directory.GetFiles(path);

            var fileprefixes = new string[]
            {
                "FW_ACS",
                "FW_BASHTEC",
                "FW_BASHTEC_TEST",
                "FW_BECKER",
                "FW_COLOMBUS",
                "FW_FI_PROD",
                "FW_FROSTIMPORT",
                "FW_LUXTEX",
                "FW_MALZAHN",
                "FW_MASTER",
                "FW_METLOG",
                "FW_NAGEL",
                "FW_PIEPENBRINK",
                "FW_TK",
                "FW_WIRA",
                "FW_WURM",
                "FW_WURM_TEST"
            };

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];

                var filename = Path.GetFileName(file);

                for (int j = 0; j < fileprefixes.Length; j++)
                {
                    var fileprefix = fileprefixes[j];

                    if (filename.StartsWith(fileprefix, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var extension = Path.GetExtension(file);

                        var directory = Path.GetDirectoryName(file);

                        var creationDate = File.GetCreationTime(file);

                        var newFilename = Path.Combine(directory, $"{fileprefix}_{creationDate:yyyy-MM-dd}_{creationDate:HH-mm}{extension}");

                        if (!File.Exists(newFilename)) File.Move(file, newFilename);
                    }
                }
            }

            Console.ReadLine();
        }
        #endregion

        #region Frostimport Datei für Lieferschein-Chargen herausfinden
        private static void GetDeliveryNoteBatchFile(string deliveryNoteId)
        {
            var files = Directory.GetFiles(".", "*.csv");

            bool fileFound = false;

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];

                using (var streamReader = new StreamReader(file, Encoding.Default))
                {
                    using (var csv = new CsvReader(streamReader, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture) { Delimiter = "," }))
                    {
                        // Header überspringen
                        csv.Read();

                        while (csv.Read())
                        {
                            var documentId = csv.GetField<string>(0);

                            if (documentId.StartsWith("E") || documentId.StartsWith("P")) documentId = documentId.Substring(1);

                            if (documentId == deliveryNoteId)
                            {
                                Console.WriteLine(file);
                                fileFound = true;
                                break;
                            }
                        }
                    }
                }

                if (fileFound) break;
            }

            Console.ReadLine();
        }
        #endregion

        #region CreateCSV
        private static void CreateCSVFile()
        {
            var items = new List<CSVExportItem>()
            {
                new CSVExportItem()
                {
                    DocumentId = "LS2301068",
                    Index = 1,
                    ClientId = "55020",
                    ArticleInformation = "310210006 KG",
                    InterfaceAmount = 480.000m,
                    Batch = "3-230420",
                    BestBeforeDate = new DateTime(2025, 4, 19),
                    SSCC = "5500204101",
                    ShippingDate = new DateTime(2023, 05, 25)
                },
                new CSVExportItem()
                {
                    DocumentId = "LS2301068",
                    Index = 1,
                    ClientId = "55020",
                    ArticleInformation = "310210006 KG",
                    InterfaceAmount = 480.000m,
                    Batch = "3-230420",
                    BestBeforeDate = new DateTime(2025, 4, 19),
                    SSCC = "5500204118",
                    ShippingDate = new DateTime(2023, 05, 25)
                },
                new CSVExportItem()
                {
                    DocumentId = "LS2301068",
                    Index = 2,
                    ClientId = "55020",
                    ArticleInformation = "310220018 KG",
                    InterfaceAmount = 504.000m,
                    Batch = "3-230502",
                    BestBeforeDate = new DateTime(2025, 4, 18),
                    SSCC = "5500220675",
                    ShippingDate = new DateTime(2023, 05, 25)
                },
            };

            ToCSV(items, ",", true, @"C:\Users\N.Groh\Documents\05 Frostimport\WA-Csv Importieren\Test WA-Csv Importieren.csv", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static void ToCSV<T>(List<T> items, string delimiter, bool createHeader, string destination, System.Globalization.CultureInfo culture)
        {
            if (!Directory.Exists(Path.GetDirectoryName(destination))) throw new Exception("Ordner gibts nich");

            var type = typeof(T);

            var csvStringBuilder = new StringBuilder();

            var properties = type.GetProperties();

            if (createHeader)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];

                    csvStringBuilder.Append(property.Name).Append(delimiter);
                }

                csvStringBuilder.AppendLine();
            }

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                for (int j = 0; j < properties.Length; j++)
                {
                    var property = properties[j];

                    var value = property.GetValue(item);

                    var formatAttribute = property.GetCustomAttribute<CSVFormatAttribute>();

                    if (formatAttribute == null) csvStringBuilder.Append(string.Format(culture, "{0}", value));
                    else csvStringBuilder.Append(formatAttribute.FormatValue(value, culture));

                    csvStringBuilder.Append(delimiter);
                }

                if (i < items.Count - 1) csvStringBuilder.AppendLine();
            }

            File.WriteAllText(destination, csvStringBuilder.ToString());
        }
        #endregion

        #region Bitshifting ausprobieren
        private static uint[] BytesToInts(byte[] bytesToConvert)
        {
            var byteShiftFactor = 0;

            uint currentUInt = 0;

            int resultUIntsArraySize = bytesToConvert.Length / 4;

            if (bytesToConvert.Length % 4 != 0) resultUIntsArraySize++;

            uint[] resultUInts = new uint[resultUIntsArraySize];

            var resultUIntsIndex = 0;

            for (int i = 0; i < bytesToConvert.Length; i++)
            {
                var fileByte = bytesToConvert[i];

                if (byteShiftFactor == 0) currentUInt = fileByte;
                else
                {
                    currentUInt <<= (8 * byteShiftFactor);

                    currentUInt += fileByte;

                    byteShiftFactor++;

                    if (byteShiftFactor > 3)
                    {
                        resultUInts[resultUIntsIndex] = currentUInt;

                        resultUIntsIndex++;

                        byteShiftFactor = 0;
                    }
                }
            }

            return resultUInts;
        }

        private static byte[] IntsToBytes(uint[] uIntsToConvert, int resultBytesArraySize)
        {
            var resultBytes = new byte[resultBytesArraySize];

            for (int i = 0; i < uIntsToConvert.Length; i++)
            {
                var uIntToConvert = uIntsToConvert[i];

                var currentResultBytesIndex = i * 4;

                for (int j = 3; j >= 0; j--)
                {
                    if (currentResultBytesIndex + j > resultBytesArraySize - 1) continue;

                    resultBytes[currentResultBytesIndex + j] = (byte)(uIntToConvert & 0x000000FF);

                    uIntToConvert >>= 8;
                }
            }

            return resultBytes;
        }
        #endregion

        #region GetSetPropertyWithExpression
        public static void LinqExpressionSetValue<T, TProperty>(T instance, Expression<Func<T, TProperty>> selectorExpression, TProperty value)
        {
            var memberSelectorExpression = selectorExpression.Body as MemberExpression;
            if (memberSelectorExpression != null)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;

                if (property != null)
                {
                    property.SetValue(instance, value, null);
                }
            }
        }

        public static TOut LinqExpressionGetValue<T, TOut>(T instance, Func<T, TOut> selectorExpression)
        {
            return selectorExpression(instance);
        }
        #endregion

        #region CSVPrettifier
        public static void CSVPrettifier()
        {
            Console.Write("Path: ");
            var path = Console.ReadLine();

            Console.Write("Delimiter: ");
            var delimiter = Console.ReadLine();

            bool numberOfRecordsInvalid = true;

            int numberOfRecords = 1;

            while (numberOfRecordsInvalid)
            {
                Console.Write("Number of Records: ");

                var numberOfRecordsString = Console.ReadLine();

                if (int.TryParse(numberOfRecordsString, out numberOfRecords)) numberOfRecordsInvalid = false;
                else
                {
                    Console.WriteLine("Invalid Input");
                    Console.WriteLine();
                }
            }

            Console.WriteLine();

            string indices = "";
            string excelColumnHeaders = "";
            string headers = "";

            var records = new List<string[]>();

            var recordLines = new List<string>();

            using (var reader = new StreamReader(path, Encoding.Default))
            {
                using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture) { Delimiter = delimiter }))
                {
                    csv.Read();

                    var headerRecord = csv.Parser.Record;

                    while (csv.Read() && records.Count < numberOfRecords)
                    {
                        try
                        {
                            records.Add(csv.Parser.Record);
                        }
                        catch { }
                    }

                    for (int i = 0; i < headerRecord.Length; i++)
                    {
                        var header = headerRecord[i];

                        int paddingLength = 0;

                        string indexHeader = i.ToString();
                        var excelColumnHeader = GetExcelColumnName(i + 1);

                        if (indexHeader.Length > paddingLength) paddingLength = i.ToString().Length;
                        if (excelColumnHeader.Length > paddingLength) paddingLength = excelColumnHeader.Length;
                        if (header.Length > paddingLength) paddingLength = header.Length;

                        for (int j = 0; j < records.Count; j++)
                        {
                            var record = records[j];

                            var recordValue = record.Length > i ? record[i] : "";

                            if (recordValue.Length > paddingLength) paddingLength = recordValue.Length;
                        }

                        indices += indexHeader.PadRight(paddingLength) + "|";
                        excelColumnHeaders += excelColumnHeader.PadRight(paddingLength) + "|";
                        headers += header.PadRight(paddingLength) + "|";

                        for (int j = 0; j < records.Count; j++)
                        {
                            var record = records[j];

                            var recordValue = record.Length > i ? record[i] : "";

                            if (i == 0) recordLines.Add(recordValue.PadRight(paddingLength) + "|");
                            else recordLines[j] += recordValue.PadRight(paddingLength) + "|";
                        }
                    }
                }
            }

            var recordLinesString = string.Join("\n", recordLines);

            var resultFileContent = string.Join("\n", indices, excelColumnHeaders, headers, recordLinesString);

            var resultFileName = Path.GetFileNameWithoutExtension(path) + "_Details";

            var resultFilePath = Path.ChangeExtension(Path.Combine(Path.GetDirectoryName(path), resultFileName), "txt");

            File.WriteAllText(resultFilePath, resultFileContent);

            Console.WriteLine("Done!");
        }

        private static string GetExcelColumnName(int columnNumber)
        {
            string columnName = "";

            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }

            return columnName;
        }
        #endregion

        #region WPF Test
        public static void WpfTest()
        {
            var UIThread = new System.Threading.Thread(StartWpfApp);
            UIThread.SetApartmentState(System.Threading.ApartmentState.STA);
            UIThread.Start();
            UIThread.Join();
        }

        public static void StartWpfApp()
        {
            var application = new System.Windows.Application();

            var mainWindow = new System.Windows.Window();

            mainWindow.Width = 500;
            mainWindow.Height = 500;

            Console.WriteLine("WPF App starting");

            application.Run(mainWindow);
        }
        #endregion

        #region ZVT Test
        static void zvtDebug()
        {
            var originalDebugOutput = "04 0F AA 27 00 04 00 00 00 00 37 65 0B 11 40 38 0C 17 41 55 0D 09 27 29 60 52 17 83 22 F1 F0 67 25 40 45 00 00 68 65 27 0F 17 00 09 87 58 27 3B 37 32 30 37 34 32 00 00 19 60 0E 23 12 8A 05 8B F0 F8 47 69 72 6F 63 61 72 64 3C F0 F1 F5 5A 61 68 6C 75 6E 67 20 65 72 66 6F 6C 67 74 88 00 58 27 06 4A 61 0C 60 0A 42 08 67 69 72 6F 63 61 72 64 45 04 22 10 00 00 2F 1E 1F 12 01 02 1F 7F 04 00 00 00 01 1F 11 01 01 1F 10 01 00 1F 7C 03 17 41 55 1F 7D 02 09 27 1F 09 13 67 25 40 45 00 00 68 65 27 0D 23 12 20 12 67 12 96 71 2F";

            var binaryCharacters = originalDebugOutput.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var joinedMessage = string.Join("", binaryCharacters);

            Console.WriteLine(joinedMessage);

            var message = Convert.FromHexString(joinedMessage);

            var debugOutput = string.Join(" ", message.Select(i => string.Format("{0:X2}", i)));

            Console.WriteLine(originalDebugOutput == debugOutput);

            WritePayResultJson(message, 5);

            var path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "result.json");

            Console.WriteLine(File.ReadAllText(path));

            Console.ReadLine();
        }

        static void WritePayResultJson(byte[] message, int startIndex)
        {
            byte? cardType = null;

            for (int i = startIndex; i < message.Length; i++)
            {
                var bmpCode = message[i];

                if (bmpCode == 0x8A)
                {
                    cardType = message[i + 1];
                    break;
                }
                else
                {
                    var length = StatusInformationFieldLengths[bmpCode];

                    // Ist es ein Feld mit variabler Länge?
                    if (length == 0)
                    {
                        var variableLengthType = VariableLengthStatusInformationFields[bmpCode];

                        switch (variableLengthType)
                        {
                            // Beispiel LLVAR: F1 F0 = 10
                            case VariableLengthType.LLVAR:
                            {
                                var f1 = message[i + 1];
                                var f2 = message[i + 2];

                                f1 &= 0x0F;
                                f2 &= 0x0F;

                                length = (byte)((f1 * 10 + f2) + 2);
                                break;
                            }// Beispiel LLLVAR: F1 F0 F3 = 103
                            case VariableLengthType.LLLVAR:
                            {
                                var f1 = message[i + 1];
                                var f2 = message[i + 2];
                                var f3 = message[i + 3];

                                f1 &= 0x0F;
                                f2 &= 0x0F;
                                f3 &= 0x0F;

                                length = (byte)((f1 * 100 + f2 * 10 + f3) + 3);

                                break;
                            }
                            case VariableLengthType.TLV:
                            {
                                // TLV-Container sollten erst kommen sobald wir unsere Information haben
                                return;
                            }// Bei nicht definiertem Fall müssen wir abbrechen
                            default: return;
                        }
                    }

                    i += length;
                }
            }

            // Wenn wir keinen Karten-Typen bekommen haben (warum auch immer) brechen wir ab
            if (cardType == null) return;

            var path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "result.json");

            var json = "{\"CardType\":" + $"\"{cardType}\"" + "}";

            File.WriteAllText(path, json);
        }

        public enum VariableLengthType
        {
            LLVAR = 0,
            LLLVAR = 1,
            TLV = 2
        }

        static readonly Dictionary<byte, byte> StatusInformationFieldLengths = new Dictionary<byte, byte>()
        {
            // Amount
            { 0x04, 6 },
            // Trace
            { 0x0B, 3 },
            // Orig. trace
            { 0x37, 3 },
            // Time
            { 0x0C, 3 },
            // Date
            { 0x0D, 2 },
            // Expiry date
            { 0x0E, 2 },
            // Sequence number
            { 0x17, 2 },
            // Payment type
            { 0x19, 1 },
            // PAN/EF_ID
            { 0x22, 0 /*LLVAR*/ },
            // Terminal-ID
            { 0x29, 4 },
            // AID
            { 0x3B, 8 },
            // CC
            { 0x49, 2 },
            // Blocked goods groups
            { 0x4C, 0 /*LLVAR*/ },
            // Receipt no.
            { 0x87, 2 },
            // Card type
            { 0x8A, 1 },
            // Card type ID
            { 0x8C, 1 },
            // Payment record
            { 0x9A, 103 },
            // AID parameter
            { 0xBA, 5 },
            // VU number
            { 0x2A, 15 },
            // Additional text
            { 0x3C, 0 /*LLLVAR*/ },
            // Result code AS
            { 0xA0, 1 },
            // Turnover no
            { 0x88, 3 },
            // Card name
            { 0x8B, 0 /*LLVAR*/ },
            // Additional data
            { 0x06, 0 /*TLV*/ },
        };

        static readonly Dictionary<byte, VariableLengthType> VariableLengthStatusInformationFields = new Dictionary<byte, VariableLengthType>()
        {
            // PAN/EF_ID
            { 0x22, VariableLengthType.LLVAR },
            // Blocked goods groups
            { 0x4C, VariableLengthType.LLVAR },
            // Additional text
            { 0x3C, VariableLengthType.LLLVAR },
            // Card name
            { 0x8B, VariableLengthType.LLVAR },
            // Additional data
            { 0x06, VariableLengthType.TLV },
        };

        static void publicKeyAusPemFile()
        {
            var certificateBytes = File.ReadAllBytes("C:\\Users\\N.Groh\\Desktop\\cert.pem");

            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificateBytes);

            var publicKey = certificate.GetPublicKeyString();

            Console.WriteLine(publicKey);

            Console.ReadLine();
        }
        #endregion
    }

    public class CSVExportItem
    {
        public string DocumentId { get; set; }

        public int Index { get; set; }

        public string ClientId { get; set; }

        public string ArticleInformation { get; set; }

        public decimal InterfaceAmount { get; set; }

        public string Batch { get; set; }

        [CSVFormat("d")]
        public DateTime BestBeforeDate { get; set; }

        public string NewSSCC { get; set; }

        public string SSCC { get; set; }

        [CSVFormat("d")]
        public DateTime ShippingDate { get; set; }

        public string NumberOFShippingUnitAA { get; set; }
    }

    public class TestClass
    {
        public string Property1 { get; set; }

        public string Property2 { get; set; }


    }

    public class DelegateAttribute
    {
        
    }

    public class CSVFormatAttribute : Attribute
    {
        public CSVFormatAttribute(string format)
        {
            Format = format;
        }

        public string Format { get; protected set; }

        public string FormatValue(object value, System.Globalization.CultureInfo culture)
        {
            return string.Format(culture, "{0:" + Format + "}", value);
        }
    }
}
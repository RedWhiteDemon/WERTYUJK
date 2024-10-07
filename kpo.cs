using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GeneticSearch
{
    // Структура для хранения данных о белке
    struct GeneticData
    {
        public string Protein;  // Название белка
        public string Organism; // Название организма
        public string AminoAcids; // Цепочка аминокислот
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Чтение данных из файлов
            List<GeneticData> sequences = ReadSequences("sequences.txt");
            ExecuteCommands("commands.txt", sequences, "genedata.txt");
        }

        // Метод для чтения данных из файла sequences.txt
        static List<GeneticData> ReadSequences(string filename)
        {
            List<GeneticData> sequences = new List<GeneticData>();
            string[] lines = File.ReadAllLines(filename);

            foreach (string line in lines)
            {
                // Данные разделены символом табуляции
                string[] parts = line.Split('\t');
                if (parts.Length == 3)
                {
                    GeneticData data = new GeneticData
                    {
                        Organism = parts[0].Trim(),
                        Protein = parts[1].Trim(),
                        AminoAcids = RLDecoding(parts[2].Trim())
                    };
                    sequences.Add(data);
                }
            }

            return sequences;
        }

        // Метод для выполнения команд из файла commands.txt
        static void ExecuteCommands(string filename, List<GeneticData> sequences, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                writer.WriteLine("YourName");  // Замени на своё имя
                writer.WriteLine("Генетический поиск");
                
                string[] lines = File.ReadAllLines(filename);
                int commandNumber = 1;

                foreach (string line in lines)
                {
                    string[] parts = line.Split('\t');
                    if (parts.Length > 0)
                    {
                        string command = parts[0];
                        string result = "";

                        switch (command)
                        {
                            case "search":
                                if (parts.Length == 2)
                                {
                                    result = Search(sequences, RLDecoding(parts[1]));
                                }
                                break;
                            case "diff":
                                if (parts.Length == 3)
                                {
                                    result = Diff(sequences, parts[1].Trim(), parts[2].Trim());
                                }
                                break;
                            case "mode":
                                if (parts.Length == 2)
                                {
                                    result = Mode(sequences, parts[1].Trim());
                                }
                                break;
                        }

                        writer.WriteLine($"---\n{commandNumber.ToString("D3")}");
                        writer.WriteLine(result);
                        commandNumber++;
                    }
                }
            }
        }

        // Метод для операции search
        static string Search(List<GeneticData> sequences, string searchSequence)
        {
            StringBuilder result = new StringBuilder();

            var matchingSequences = sequences.Where(seq => seq.AminoAcids.Contains(searchSequence)).ToList();

            if (matchingSequences.Count > 0)
            {
                foreach (var seq in matchingSequences)
                {
                    result.AppendLine($"{seq.Organism} {seq.Protein}");
                }
            }
            else
            {
                result.AppendLine("NOT FOUND");
            }

            return result.ToString();
        }

        // Метод для операции diff
        static string Diff(List<GeneticData> sequences, string protein1, string protein2)
        {
            var seq1 = sequences.FirstOrDefault(seq => seq.Protein == protein1);
            var seq2 = sequences.FirstOrDefault(seq => seq.Protein == protein2);

            if (seq1.Protein == null || seq2.Protein == null)
            {
                return $"amino-acids difference:\nMISSING: {(seq1.Protein == null ? protein1 : "")} {(seq2.Protein == null ? protein2 : "")}".Trim();
            }

            int length = Math.Min(seq1.AminoAcids.Length, seq2.AminoAcids.Length);
            int differences = Math.Abs(seq1.AminoAcids.Length - seq2.AminoAcids.Length);

            for (int i = 0; i < length; i++)
            {
                if (seq1.AminoAcids[i] != seq2.AminoAcids[i])
                {
                    differences++;
                }
            }

            return $"amino-acids difference: {differences}";
        }

        // Метод для операции mode
        static string Mode(List<GeneticData> sequences, string proteinName)
        {
            var protein = sequences.FirstOrDefault(seq => seq.Protein == proteinName);

            if (protein.Protein == null)
            {
                return $"amino-acid occurs:\nMISSING: {proteinName}";
            }

            // Подсчет частоты каждой аминокислоты
            var frequencies = protein.AminoAcids.GroupBy(c => c)
                                                .Select(group => new { AminoAcid = group.Key, Count = group.Count() })
                                                .OrderByDescending(x => x.Count)
                                                .ThenBy(x => x.AminoAcid)
                                                .ToList();

            var mostFrequent = frequencies.First();
            return $"amino-acid occurs: {mostFrequent.AminoAcid} {mostFrequent.Count}";
        }

        // Метод декодирования RLE
        static string RLDecoding(string encoded)
        {
            StringBuilder decoded = new StringBuilder();
            Regex regex = new Regex(@"(\d*)([A-Z])");

            foreach (Match match in regex.Matches(encoded))
            {
                int count = string.IsNullOrEmpty(match.Groups[1].Value) ? 1 : int.Parse(match.Groups[1].Value);
                decoded.Append(new string(match.Groups[2].Value[0], count));
            }

            return decoded.ToString();
        }

        // Метод кодирования RLE
        static string RLEncoding(string aminoAcids)
        {
            StringBuilder encoded = new StringBuilder();
            int count = 1;

            for (int i = 1; i < aminoAcids.Length; i++)
            {
                if (aminoAcids[i] == aminoAcids[i - 1])
                {
                    count++;
                }
                else
                {
                    if (count > 1)
                    {
                        encoded.Append(count);
                    }
                    encoded.Append(aminoAcids[i - 1]);
                    count = 1;
                }
            }

            if (count > 1)
            {
                encoded.Append(count);
            }
            encoded.Append(aminoAcids[^1]);

            return encoded.ToString();
        }
    }
}

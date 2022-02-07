using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CovidStatistics
{
    public partial class Form1 : Form
    {
        private List<CovidStat> covidStats = new List<CovidStat>();
        private bool dataLoaded;

        public Form1()
        {
            InitializeComponent();
        }

        // Eventi

        private void btnLoadCSV_Click(object sender, EventArgs e)
        {
            LoadCSV();
            UpdateCombo();
        }

        private void btnMean_Click(object sender, EventArgs e)
        {
            if (dataLoaded)
                txtMean.Text = "$ " + Math.Round(CalcMean(), 2).ToString("#,##0.00");
            else
                MessageBox.Show("Caricare il file CSV");
        }

        private void btnStdDeviation_Click(object sender, EventArgs e)
        {
            if (dataLoaded)
                txtStdDeviation.Text = "$ " + Math.Round(CalcStdDeviation(), 2).ToString("#,##0.00");
            else
                MessageBox.Show("Caricare il file CSV");
        }

        private void btnFreqByComm_Click(object sender, EventArgs e)
        {
            txtFrequency.Text = string.Empty;

            if (dataLoaded)
            {
                List<Tuple<string, double>> values = CalcFrequencyByCommodity();
                foreach (var item in values)
                {
                    txtFrequency.Text += Math.Round(item.Item2, 2).ToString("###,##0").PadLeft(7) + " Items\t" + item.Item1 + "\r\n";
                }
            }
            else
                MessageBox.Show("Caricare il file CSV");
        }

        private void btnFreByYear_Click(object sender, EventArgs e)
        {
            txtFrequency.Text = string.Empty;

            if (dataLoaded)
            {
                List<Tuple<string, double>> values = CalcFrequencyByYear();
                foreach (var item in values)
                {
                    txtFrequency.Text += Math.Round(item.Item2, 2).ToString("###,##0").PadLeft(7) + " Items\t" + item.Item1 + "\r\n";
                }
            }
            else
                MessageBox.Show("Caricare il file CSV");
        }

        // Metodi

        private void LoadCSV()
        {
            string[] data;
            string dataLine;

            var lines = File.ReadAllLines(@"Dati\effects-of-covid-19-on-trade.csv").Skip(1);
            foreach (var line in lines)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        dataLine = line;
                        // Rintraccia i campi delimitati da doppi apici (es.: "Milk powder, butter, and cheese") che contengono virgole e
                        // rimpiazza le virgole con punto e virgola
                        while (dataLine.Contains('"'))
                        {
                            int p1 = line.IndexOf('"');
                            int p2 = line.IndexOf('"', p1 + 1);
                            dataLine = line.Substring(0, p1) + line.Substring(p1 + 1, p2 - p1 - 1).Replace(',', ';') + line.Substring(p2 + 1); 
                        }

                        data = dataLine.Split(',');

                        // Aggiunge un elemento della classe alla collezione
                        covidStats.Add(new CovidStat()
                        {
                            Direction = data[0].Trim(),
                            Year = int.Parse(data[1].Trim()),
                            Date = DateTime.Parse(data[2].Trim()),
                            Weekday = data[3].Trim(),
                            Country = data[4].Trim(),
                            Commodity = data[5].Trim(),
                            Transport_Mode = data[6].Trim(),
                            Measure = data[7].Trim(),
                            Value = double.Parse(data[8].Trim()),
                            Cumulative = double.Parse(data[9].Trim())
                        });

                        dataLoaded = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore.\n\nError message: {ex.Message}\n\n" + $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        private void UpdateCombo()
        {
            if (dataLoaded)
            {
                var countries = covidStats.GroupBy(x => x.Country).Select(x => x.FirstOrDefault()).ToList();

                cmbCountries.Items.Clear();
                foreach (var item in countries)
                    cmbCountries.Items.Add(item.Country);
                cmbCountries.SelectedIndex = 0;
            }
        }

        private double CalcMean()
        {
            double mean = 0;

            // Filtra l'elenco in base alle selezioni in maschera
            var stats = covidStats.Where(x => x.Country == cmbCountries.SelectedItem.ToString()).ToList();
            if (stats.Any())
            {
                foreach (CovidStat covidStat in stats)
                    mean += covidStat.Value;

                mean = mean / stats.Count();
            }

            return mean;
        }

        private double CalcStdDeviation()
        {
            double stdDeviation = 0;

            // Filtra l'elenco in base alle selezioni in maschera
            var stats = covidStats.Where(x => x.Country == cmbCountries.SelectedItem.ToString()).ToList();

            var values = new List<double>();
            foreach (CovidStat covidStat in stats)
                values.Add(covidStat.Value);

            if (values.Any())
            {
                double avg = values.Average();
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                stdDeviation = Math.Sqrt((sum) / (values.Count() - 1));
            }

            return stdDeviation;
        }

        private List<Tuple<string, double>> CalcFrequencyByCommodity()
        {
            List<Tuple<string, double>> values = new List<Tuple<string, double>>();

            // Filtra l'elenco in base alle selezioni in maschera
            var commodities = covidStats.GroupBy(x => x.Commodity).Select(x => x.FirstOrDefault()).ToList();
            foreach (var cmd in commodities)
            {
                var stats = covidStats.Where(x => x.Country == cmbCountries.SelectedItem.ToString() && x.Commodity == cmd.Commodity).ToList();
                values.Add(new Tuple<string, double>(cmd.Commodity, stats.Count()));
            }
            
            return values;
        }

        private List<Tuple<string, double>> CalcFrequencyByYear()
        {
            List<Tuple<string, double>> values = new List<Tuple<string, double>>();

            // Filtra l'elenco in base alle selezioni in maschera
            var years = covidStats.GroupBy(x => x.Year).Select(x => x.FirstOrDefault()).ToList();
            foreach (var value in years)
            {
                var stats = covidStats.Where(x => x.Country == cmbCountries.SelectedItem.ToString() && x.Year == value.Year).ToList();
                values.Add(new Tuple<string, double>(value.Year.ToString(), stats.Count()));
            }

            return values;
        }
    }
}

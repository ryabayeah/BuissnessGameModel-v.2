using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        
        Exchanger exchanger = new Exchanger(" $");
        public Form1()
        {   
            InitializeComponent();
            bDollar.BackColor = Color.Gray;
            bSell.Enabled = false;
        }

        
        private void button1_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
                button1.Text = "Start Timer";
            }
            else
            {
                exchanger.rate = (double)nRate.Value;
                timer1.Start();
                button1.Text = "Stop Timer";
            }
            
        }

        private void bBuy_Click(object sender, EventArgs e)
        {
            (tCash.Text, tBoughtCash.Text) = exchanger.Buy((double)nRate.Value, (double)nWantCash.Value);
            (bBuy.Enabled, bSell.Enabled) = exchanger.CheckCash();

            if (exchanger.cash <= 0 & exchanger.boughtCash <= 0)
            {
                bBuy.Enabled = bSell.Enabled = false;
                lCash.Text = "You loose!";
                return;
            }
        }

        private void bSell_Click(object sender, EventArgs e)
        {
            (tCash.Text, tBoughtCash.Text) = exchanger.Sell((double)nRate.Value, (double)nWantCash.Value);
            (bBuy.Enabled, bSell.Enabled) = exchanger.CheckCash();
        }

        private void bEuro_Click(object sender, EventArgs e)
        {
            exchanger.Restart(" €");
            rePaint();
            bEuro.BackColor = Color.Gray;
            bDollar.BackColor = Color.White;
        }

        private void bDollar_Click(object sender, EventArgs e)
        {
            exchanger.Restart(" $");
            rePaint();
            bDollar.BackColor = Color.Gray;
            bEuro.BackColor = Color.White;
        }

        public void rePaint()
        {
            (tCash.Text, tBoughtCash.Text) = exchanger.GetFormatedCash();
            chart1.Series[0].Points.Clear();
            nRate.Value = (decimal)exchanger.GetRate();
            exchanger.rate = (double)nRate.Value;
            chart1.Series[0].Points.AddXY(0, (double)nRate.Value);
            bDollar.BackColor = Color.Gray;
            bEuro.BackColor = Color.White;
            chart1.ChartAreas[0].AxisX.ScrollBar.Axis.ScaleView.Position = 0;
            chart1.ChartAreas[0].AxisX.ScrollBar.Enabled = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            exchanger.rate = exchanger.GetRate();
            chart1.Series[0].Points.AddXY(exchanger.days++, exchanger.rate);
            chart1.ChartAreas[0].AxisX.ScaleView.Size = 30;
            nRate.Value = (decimal)exchanger.rate;

            if (exchanger.days <= 30) return;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            chart1.ChartAreas[0].AxisX.ScrollBar.Axis.ScaleView.Position = exchanger.days - 30;
        }
    }

    class Exchanger
    {
        public const double k = 0.1;
        public int days;
        public double rate;
        Random random = new Random();
        public string currency;
        public double cash;
        public double boughtCash;
        public string currencyCash;

        private const double Sigma = 0.092;
        private const double Mu = 0.005;
        static double dt = 0.1;

        public Exchanger(string Currency)
        {
            currency = Currency;
            cash = 1000;
            boughtCash = 0;
            days = 1;
            currencyCash = " ₽";
        }

        public double GetRate() {
            double RndValue = GetNormalRandomValue();
            
            rate *= Math.Exp((Mu - Math.Pow(Sigma, 2) / 2) * dt + Sigma * Math.Sqrt(dt) * RndValue);
            days++;
            
            return rate;
        }

        private double GetNormalRandomValue()
        {
            double u1 = 1 - random.NextDouble();
            double u2 = 1 - random.NextDouble();
            if (random.NextDouble() > 0.5)
            {
                return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            }
            else
            {
                return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            }

        }

        public (string, string) GetFormatedCash() { return (cash + currencyCash, boughtCash + currency); }

        public (string, string) Buy(double rate, double quantity)
        {
            if (cash - Math.Round(rate * quantity, 2) < 0){ quantity = (int)(cash / rate); };
            cash -= Math.Round(rate * quantity, 2);
            boughtCash += quantity;
            return GetFormatedCash();
        }

        public ( string, string) Sell(double rate, double quantity)
        {
            if (boughtCash - quantity < 0) { quantity = boughtCash; }
            Console.WriteLine(quantity);
            cash += Math.Round(rate * quantity, 2);
            boughtCash -= quantity;
            return GetFormatedCash();
        }

        public (bool, bool) CheckCash()
        {
            bool cPass = true , bPass = true;
            if (cash < 0) { cPass = false; }
            if (boughtCash <= 0) { bPass = false; }

            return (cPass, bPass);
        }
        public void Restart(string Currency)
        {
            currency = Currency;
            cash = 1000;
            days = 1;
            boughtCash = 0;
            currencyCash = " ₽";
        }
    }
}

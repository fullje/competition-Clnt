using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EasyCrypto;
using MySql.Data.MySqlClient;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using HtmlAgilityPack;
using System.Windows.Threading;

/* - logowanie przy uzyciu danych z json'a oraz sprawdzenie czasu / DONE
 * - Pobieranie pytan do okienka wraz z odpowiedzia / DONE
 * - sprawdzic edytorem HEX czy da sie pobrac dane do bazy danych albo edytowac cokolwiek / TO CHECK
 * 
 * 
 */

namespace Matura_z_Gier___Client
{
   
    public partial class MainWindow : Window
    {
        bool timeFlag = false;
        public MainWindow()
        {

            InitializeComponent();
            loadAll();

            //MessageBox.Show(getTime().ToString("yyyy:dd:MM - H:mm:ss"));
        }
        
        private void setTime(object sender, EventArgs e)
        {
            try
            {
                connectDB conn = new connectDB();
                string sqlDate = null;

                //Send actual time to textBlock
                actualTimeBlock.Text = getTime().ToString();

                //Get Time from DB
                sqlDate = conn.selectTimeDB().Rows[0][0].ToString();
                DateTime time = DateTime.Parse(sqlDate);

                //Porównanie tylko w sekundach bo bedzie problem z godiznami 15:03 != 14:30
                if ((Convert.ToInt32(getTime().ToString("mm"))) >= (Convert.ToInt32(time.ToString("mm"))))
                {
                    startTime(true);

                    //Set enable button to start
                    enterEg.IsEnabled = true;
                    tabGame.Visibility = Visibility.Visible;
                }
            }catch (Exception xe)
            {

            }
            
        }


        DispatcherTimer timer = new DispatcherTimer();
        private void startTime(bool what)
        {
            if (what == false)
            {
                timer.Interval = new TimeSpan(0, 0, 1);
                timer.Tick += setTime;
                timer.Start();
            }

            if (what == true)
            {
                MessageBox.Show("Matura się już rozpoczęła!");
                timer.Stop();
            }
        }

        private void loadAll()
        {
            
            tabGame.Visibility = Visibility.Hidden;
            enterEg.IsEnabled = false;
            timeToStartBlock.Visibility = Visibility.Hidden;
        }

        private DateTime getTime() 
        {
            var url = @"https://www.unixtimestamp.com/";
            var web = new HtmlWeb();
            var webData = web.Load(url);

            int timeInSec = 0;

            string trash = null;
            string unixSec = null;

            var htmlNode = webData.DocumentNode.SelectSingleNode("//body/div[1]/div[1]/div[2]/div[1]/div[1]/div[1]/p/h3");
            if (null != htmlNode)
            {
                var htmlRemove = webData.DocumentNode.SelectSingleNode("//body/div[1]/div[1]/div[2]/div[1]/div[1]/div[1]/p/h3/small");

                trash = htmlRemove.OuterHtml;
                unixSec = htmlNode.InnerHtml;
                //MessageBox.Show(htmlNode.InnerHtml);
            }

            StringBuilder sb = new StringBuilder(unixSec);

            timeInSec = Convert.ToInt32(sb.Replace(trash, "").ToString());

            DateTime dt = new DateTime(1970, 1, 1, 2, 0, 0).AddSeconds(timeInSec);
            
            //MessageBox.Show(dt.ToString("yyyy:dd:MM - H:mm:ss"));
            //Ewentualnie timeInSec zwaraca pelne sekundy od 1970

            return dt;
        }

        
        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            connectDB conn = new connectDB();
            player p = new player();

            string sqlDate = null;

            if (loginBox.Text.ToString() != null && emailBox.Text.ToString() != null && passwordBox.Password != null)
            {
                try
                {
                    p.game(loginBox.Text.ToString(), emailBox.Text.ToString());

                    conn.decodeConnection(passwordBox.Password);
                    MessageBox.Show("Pomyslnie zarejestrowano uzytkownika! Czekaj na start matury!");
                    loginBtn.IsEnabled = false;
                    
                    timeToStartBlock.Visibility = Visibility.Visible;

                    sqlDate = conn.selectTimeDB().Rows[0][0].ToString();

                    DateTime time = DateTime.Parse(sqlDate);

                    //When start?
                    timeToStartBlock.Text = time.ToString();
                    //MessageBox.Show(time.ToString());
                    startTime(false);
                }
                catch (Exception xe)
                {
                    MessageBox.Show("Error: " + xe.Message);
                }
            }

        }

        private void testbtn_Click(object sender, RoutedEventArgs e)
        {
            player p = new player();

            p.showStats();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            connectDB con = new connectDB();
            try
            {
                //con.getQ(1);
                DataTable dt = con.getQ(2) as DataTable;
                MessageBox.Show(dt.Rows[0]["TrescPytania"].ToString());

            }
            catch (Exception xe)
            {
                MessageBox.Show(xe.Message);
            }
        }

        private void enterEg_Click(object sender, RoutedEventArgs e)
        {
            tabMain.SelectedIndex = 1;
            tabGame.Visibility = Visibility.Visible;

            setQuestion(1);
        }

        private void setQuestion(int qN)
        {
            connectDB con = new connectDB();
            player p = new player();
            DataTable dt = con.getQ(qN) as DataTable;
            string q = "SELECT MAX(NumerPytania) FROM matura;";
            DataTable cdt = con.customSelect(q);

            // Select MAX("NumerPytania") FROM matura;
            allQuestion.Text = cdt.Rows[0][0].ToString();

            qNumberBox.Text = dt.Rows[0]["NumerPytania"].ToString();
            if(Convert.ToInt32(qNumberBox.Text) == Convert.ToInt32(allQuestion.Text))
            {
                finishBtn.IsEnabled = true;
            }

            questionBox.Text = dt.Rows[0]["TrescPytania"].ToString();
            answerA.Text = dt.Rows[0]["OdpowiedzA"].ToString();
            answerB.Text = dt.Rows[0]["OdpowiedzB"].ToString();
            answerC.Text = dt.Rows[0]["OdpowiedzC"].ToString();
            answerD.Text = dt.Rows[0]["OdpowiedzD"].ToString();

        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            int number = Convert.ToInt32(qNumberBox.Text.ToString());
            MessageBox.Show("Pytanie: " + number.ToString());
            number++;

            if(number <= Convert.ToInt32(allQuestion.Text.ToString()))
            {
                setQuestion(number);
            }
        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            int number = Convert.ToInt32(qNumberBox.Text.ToString());
            MessageBox.Show("Pytanie: " + number.ToString());
            number--;

            if (number >= 1)
            {
                setQuestion(number);
            }
      
        }

        private void ARadial_Checked(object sender, RoutedEventArgs e)
        {
            connectDB con = new connectDB();
            player p = new player();
            int qN = Convert.ToInt32(qNumberBox.Text.ToString());
            DataTable dt = con.getQ(qN) as DataTable;
            string z = dt.Rows[0]["PoprawnaOdpowiedz"].ToString();

            if (ARadial.Content.ToString() == z)
            {
                MessageBox.Show("Correct!");
                p.addPoints();
            }
        }

        private void BRadial_Checked(object sender, RoutedEventArgs e)
        {
            connectDB con = new connectDB();
            player p = new player();
            int qN = Convert.ToInt32(qNumberBox.Text.ToString());
            DataTable dt = con.getQ(qN) as DataTable;
            string z = dt.Rows[0]["PoprawnaOdpowiedz"].ToString();

            if (BRadial.Content.ToString() == z)
            {
                MessageBox.Show("Correct!");
                p.addPoints();
            }
        }

        private void CRadial_Checked(object sender, RoutedEventArgs e)
        {
            connectDB con = new connectDB();
            player p = new player();
            int qN = Convert.ToInt32(qNumberBox.Text.ToString());
            DataTable dt = con.getQ(qN) as DataTable;
            string z = dt.Rows[0]["PoprawnaOdpowiedz"].ToString();

            if (CRadial.Content.ToString() == z)
            {
                MessageBox.Show("Correct!");
                p.addPoints();
            }
        }

        private void DRadial_Checked(object sender, RoutedEventArgs e)
        {
            connectDB con = new connectDB();
            player p = new player();
            int qN = Convert.ToInt32(qNumberBox.Text.ToString());
            DataTable dt = con.getQ(qN) as DataTable;
            string z = dt.Rows[0]["PoprawnaOdpowiedz"].ToString();

            if (DRadial.Content.ToString() == z)
            {
                MessageBox.Show("Correct!");
                p.addPoints();
            }
        }

        private void finishBtn_Click(object sender, RoutedEventArgs e)
        {
            player p = new player();
            string time = getTime().ToString("yyyy:dd:MM - H:mm:ss");
            p.sendStats(time);
        }
    }



    class player
    {
        static string nick = null;
        static string email = null;
        static int points = 0;

        public void game(string l, string e)
        {
            nick = l;
            email = e;
        }

        public void addPoints()
        {
            points++;
        }

        public void showStats()
        {
            MessageBox.Show("Gracz: " + nick + " email: " + email + " punktów: " + points);
        }

        public void sendStats(string time)
        {
            connectDB con = new connectDB();
            MessageBox.Show("Gracz: " + nick + " email: " + email + " punktów: " + points + " czas: " + time);

            con.insertDB(nick, email, points, time);
        }
    }

    class connectDB
    {
        static string setConnection;
        static string serverAddr = null;
        static string serverPass = null;
        MySqlConnection clientDB;

        public void connection(string server, string password)
        {
            setConnection = "SERVER=" + server + ";DATABASE=matura;User ID=root;Password=" + password + ";SSLmode=none";
            clientDB = new MySqlConnection(setConnection);
            clientDB.Open();

            clientDB.Close();
        }

        public void decodeConnection(string password) // Args, patch to the file
        {
            string st = File.ReadAllText(@"C:\Projekty\TicketApp\MaturaZGierKLIENT\keys.json");

            var jPerson = JsonConvert.DeserializeObject<dynamic>(st);

            serverAddr = jPerson.kycu.ToString();
            serverAddr = AesEncryption.DecryptWithPassword(serverAddr, password);
            serverPass = jPerson.losiu.ToString();
            serverPass = AesEncryption.DecryptWithPassword(serverPass, password);

            //Show(serverAddr + " --- " + serverPass);
        }

        public DataTable getQ(int q)
        {
            connection(serverAddr, serverPass);
            string query = "SELECT * FROM matura WHERE (NumerPytania=" + q + ");";
            MySqlCommand commandSelect = new MySqlCommand(query, clientDB);
            MySqlDataAdapter adp = new MySqlDataAdapter(commandSelect);
            DataTable dt = new DataTable();
            adp.Fill(dt);
            clientDB.Open();


            clientDB.Close();

            return dt;
        }
        
        public void insertDB(string nick, string email, int correct, string time)
        {
            connection(serverAddr, serverPass);
            string query = "INSERT INTO konkurs (Nick, email, CorrectAnswers, Time) VALUES ('" + nick + "','" + email + "'" + ",'" + correct + "'" + ",'" + time + "');";
            //MessageBox.Show(query);
            MySqlCommand commandInsert = new MySqlCommand(query, clientDB);
            MySqlDataReader reader;
            clientDB.Open();

            reader = commandInsert.ExecuteReader();

            reader.Close();
            clientDB.Close();
        }

        public DataTable customSelect(string query)
        {
            connection(serverAddr, serverPass);
            
            MySqlCommand commandSelect = new MySqlCommand(query, clientDB);
            MySqlDataAdapter adp = new MySqlDataAdapter(commandSelect);
            DataTable dt = new DataTable();
            adp.Fill(dt);
            clientDB.Open();


            clientDB.Close();

            return dt;
        }

        public DataTable selectTimeDB()
        {
            connection(serverAddr, serverPass);
            //string query = "SELECT * FROM eventStart";
            string query = "select DATE_FORMAT(date, '%Y-%m-%d %H:%i:%s') from eventStart";
            MySqlCommand commandSelect = new MySqlCommand(query, clientDB);
            MySqlDataAdapter adp = new MySqlDataAdapter(commandSelect);
            DataTable dt = new DataTable();
            adp.Fill(dt);

            clientDB.Open();

            clientDB.Close();

            return dt;
        }

    }


}


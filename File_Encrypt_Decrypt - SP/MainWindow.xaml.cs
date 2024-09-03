using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace File_Encrypt_Decrypt___SP;

public partial class MainWindow : Window
{
    private CancellationTokenSource _cts;
    public string Parol = "44";
    public bool CheckStart = true;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void SelectFileButton_Click(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
        if (openFileDialog.ShowDialog() == true)
        {
            FilePathTextBox.Text = openFileDialog.FileName;
        }
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        string filePath = FilePathTextBox.Text;
        string password = PasswordBox.Password;

        if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Please Selected File or Password !");
            return;
        }

        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;

        if(password != Parol)
        {
            MessageBox.Show($"Password -> {Parol}");
            return;
        }

        if (EncryptRadioButton.IsChecked == true)
        {
            ThreadPool.QueueUserWorkItem(_ => EncryptFile(filePath, password, token));
        }
        else if (DecryptRadioButton.IsChecked == true)
        {
            ThreadPool.QueueUserWorkItem(_ => DecryptFile(filePath, password, token));
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        CheckStart = false;
        for (int i = 0; i < ProgressBar.Value; i++)
        {
            if (ProgressBar.Value == 0) return;
            Dispatcher.Invoke(() => 
            {
                ProgressBar.Value --;
                Thread.Sleep(10);
                btnStart.IsEnabled = false; 
                btnCancel.IsEnabled = false;
            });
        }

        Dispatcher.Invoke(() =>
        {
            ProgressBar.Value = 0;
            btnStart.IsEnabled = true;
            btnCancel.IsEnabled = true;
        });
        _cts?.Cancel();
        MessageBox.Show("Canel");
    }

    private void EncryptFile(string filePath, string password, CancellationToken token)
    {
        try
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = 0;
                ProgressBar.Maximum = fileBytes.Length;
                btnStart.IsEnabled = false;
            });

            for (int i = 0; i < fileBytes.Length; i++)
            {
                if (CheckStart)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (CheckStart)
                        {
                            ProgressBar.Value++;
                            Thread.Sleep(10);
                            _cts.Cancel();
                        }
                        else
                        {
                            ProgressBar.Value = 0;
                            FilePathTextBox.Text = null;
                            PasswordBox.Password = null;
                            btnStart.IsEnabled = true;
                            btnCancel.IsEnabled = true;
                            return;
                        }
                    });
                    fileBytes[i] = (byte)(fileBytes[i] ^ passwordBytes[i % passwordBytes.Length]);
                }
                else return;
            }


            Dispatcher.Invoke(() => {

                ProgressBar.Value = 0;
                FilePathTextBox.Text = null;
                PasswordBox.Password = null;
                btnStart.IsEnabled = true;
            });

            File.WriteAllBytes(filePath, fileBytes);
            MessageBox.Show("Success");

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void DecryptFile(string filePath, string password, CancellationToken token)
    {
        EncryptFile(filePath, password, token);
    }
}
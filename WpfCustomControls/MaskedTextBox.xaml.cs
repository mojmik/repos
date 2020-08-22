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

namespace WpfCustomControls {
    /// <summary>
    /// Interaction logic for MaskedTextBox.xaml
    /// </summary>
    public partial class MaskedTextBox : UserControl {
        public MaskedTextBox() {
            InitializeComponent();
        }
        public enum BadValueHandler {
            RedBox,
            MessageBox
        }

        private BadValueHandler _valueHandler;
        public BadValueHandler valueHandler {
            get {
                return _valueHandler;
            }
            set {
                _valueHandler = value;
            }
        }
        private string _mask;
        public string mask {
            get {
                return _mask;
            }
            set {
                System.Collections.Generic.List<string> poleZnaku = new System.Collections.Generic.List<string>();
                for (int i = 0; i < value.Length; i++) {
                    poleZnaku.Add(value.Substring(i, 1));
                }
                foreach (var znak in poleZnaku) {
                    switch (znak) {
                        case "0":
                            break;
                        case "a":
                            break;
                        case ".":
                            break;
                        default:
                            throw new FormatException("Neplatný znak v masce");
                    }
                }
                _mask = value;
            }
        }
        public string Text {
            get {
                return generalTextBox.Text;
            }
            set {
                generalTextBox.Text = value;
            }
        }

        private void generalTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (Validate()) {
                generalTextBox.Background = Brushes.White;
            }
            else {
                switch (valueHandler) {
                    case BadValueHandler.RedBox:
                        generalTextBox.Background = Brushes.OrangeRed;
                        break;
                    case BadValueHandler.MessageBox:
                        if (mask.Length == generalTextBox.Text.Length) {
                            MessageBox.Show("Zadali jste neplatnou hodnotu, zadejte hodnotu ve formátu: " + mask, "Neplatná hodnota", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        break;
                }
            }
        }
        private bool Validate() {
            string writedText = generalTextBox.Text;
            string maska = this.mask;
            if (writedText.Length != maska.Length) {
                return false;
            }
            for (int i = 0; i < maska.Length; i++) {
                string znakMasky = maska.Substring(i, 1);
                string znakVRetezci = writedText.Substring(i, 1);
                switch (znakMasky) {
                    case "0":
                        try {
                            int testValue = int.Parse(znakVRetezci) / 2;
                        }
                        catch (Exception) {
                            return false;
                        }
                        break;
                    case "a":
                        int asc = (int)char.Parse(znakVRetezci);
                        if (!((asc >= 65 && asc <= 90) || (asc >= 97 && asc <= 122))) {
                            return false;
                        }
                        break;
                    case ".":
                        int dotAsc = (int)char.Parse(znakVRetezci);
                        if (!(dotAsc == 46 || dotAsc == 44)) {
                            return false;
                        }
                        break;
                    default:
                        throw new FormatException("Naplatný formát masky");
                }
            }
            return true;
        }
    }
}

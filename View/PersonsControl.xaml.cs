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

namespace View {
    /// <summary>
    /// Interaction logic for PersonsControl.xaml
    /// </summary>
    public partial class PersonsControl : UserControl {
        public PersonsControl() {
            InitializeComponent();
        }
        Manager manager;
        public void Init(Manager manager) {
            this.manager = manager;
            dataGridPersons.ItemsSource = manager.Persons;
        }

        private void btnNew_Click(object sender, RoutedEventArgs e) {
            SetPersonButtons(false, false, true, false);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e) {
            SetPersonButtons(false, false, true, false);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            SetPersonButtons(true, true, false, true);
        }

        private void btnDel_Click(object sender, RoutedEventArgs e) {
            SetPersonButtons(true, true, false, false);
        }
        private void SetPersonButtons(bool newPerson, bool edit, bool save, bool delete) {
            btnNew.IsEnabled = newPerson;
            btnEdit.IsEnabled = edit;
            btnSave.IsEnabled = save;
            btnDel.IsEnabled = delete;
        }
    }
}

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
    /// Interaction logic for InvoicesControl.xaml
    /// </summary>
    public partial class InvoicesControl : UserControl {
        private Manager manager;
        public InvoicesControl() {
            InitializeComponent();
        }

        public void Init(Manager manager) {
            this.manager = manager;
            dataGrid.ItemsSource = manager.Invoices;
            cmbCustomer.ItemsSource = manager.Persons.Select(p => p.Surname + " " + p.Name);
            cmbSupplier.ItemsSource = manager.Persons.Select(p => p.Surname + " " + p.Name); ;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            SetInvoiceButtons(true, false, true);
        }

        private void btnNew_Click(object sender, RoutedEventArgs e) {
            SetInvoiceButtons(false, true, false);
        }

        private void btnDel_Click(object sender, RoutedEventArgs e) {
            SetInvoiceButtons(true, false, true);
        }

        private void SetInvoiceButtons(bool newInvoice, bool save, bool delete) {
            btnNew.IsEnabled = newInvoice;
            btnSave.IsEnabled = save;
            btnDel.IsEnabled = delete;
        }


    }
}

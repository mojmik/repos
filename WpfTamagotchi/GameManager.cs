using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfTamagotchi {
    class GameManager : INotifyPropertyChanged {
        Pet pet;
        public event PropertyChangedEventHandler PropertyChanged;
        BackgroundWorker sleepBackgroundWorker;
        BackgroundWorker foodBackgroundWorker;
        BackgroundWorker walkBackgroundWorker;
        BackgroundWorker hygieneBackgroundWorker;

        public int SleepProgress { get; set; }
        public int FoodProgress { get; set; }
        public int HygieneProgress { get; set; }
        public int WalkProgress { get; set; }

        int foodNeed=500;
        int sleepNeed = 1200;
        int walkNeed = 800;
        int hygieneNeed = 2000;


        public string PetImage { get; set; }
        
        bool petMood=true;
        bool delamPotrebu = false;

        public string infoText { get; set; }
        public bool sleepClicked,walkClicked,foodClicked,hygieneClicked;

        public GameManager() {
            pet = new Pet();
            NastavVychoziObrazek();
            MikPropertyChanged(nameof(PetImage));
            sleepBackgroundWorker = new BackgroundWorker() {
                WorkerReportsProgress = true
            };
            sleepBackgroundWorker.DoWork += SleepBackgroundWorker_DoWork;
            sleepBackgroundWorker.ProgressChanged += SleepBackgroundWorker_ProgressChanged;
            sleepBackgroundWorker.RunWorkerCompleted += SleepBackgroundWorker_RunWorkerCompleted;
            SleepProgress = pet.Spanek;
            sleepBackgroundWorker.RunWorkerAsync(10);

            foodBackgroundWorker = new BackgroundWorker() {
                WorkerReportsProgress = true
            };
            foodBackgroundWorker.DoWork += FoodBackgroundWorker_DoWork;
            foodBackgroundWorker.ProgressChanged += FoodBackgroundWorker_ProgressChanged;
            foodBackgroundWorker.RunWorkerCompleted += FoodBackgroundWorker_RunWorkerCompleted;
            FoodProgress = pet.Food;
            foodBackgroundWorker.RunWorkerAsync(10);

            hygieneBackgroundWorker = new BackgroundWorker() {
                WorkerReportsProgress = true
            };
            hygieneBackgroundWorker.DoWork += HygieneBackgroundWorker_DoWork;
            hygieneBackgroundWorker.ProgressChanged += HygieneBackgroundWorker_ProgressChanged;
            hygieneBackgroundWorker.RunWorkerCompleted += HygieneBackgroundWorker_RunWorkerCompleted;
            HygieneProgress = pet.Hygiene;
            hygieneBackgroundWorker.RunWorkerAsync(10);

            walkBackgroundWorker = new BackgroundWorker() {
                WorkerReportsProgress = true
            };
            walkBackgroundWorker.DoWork += WalkBackgroundWorker_DoWork;
            walkBackgroundWorker.ProgressChanged += WalkBackgroundWorker_ProgressChanged;
            walkBackgroundWorker.RunWorkerCompleted += WalkBackgroundWorker_RunWorkerCompleted;
            WalkProgress = pet.Walk;
            walkBackgroundWorker.RunWorkerAsync(10);
        }
        private void SleepBackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            int pocet = (int)e.Argument;
            for (int i = pet.Spanek; i >= 0; i-- ) {
                if (sleepClicked) {
                    pet.Spanek = 99;
                    i = pet.Spanek;
                    sleepClicked = false;
                }
                pet.Spanek = i;
                Thread.Sleep(sleepNeed);                
                int procent = (int)Math.Round(100*i/(double)100);
                sleepBackgroundWorker.ReportProgress(procent);                       
            }
        }
        private void SleepBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            SleepProgress = e.ProgressPercentage;
            CheckHappy();
            MikPropertyChanged(nameof(SleepProgress));
        }
        private void SleepBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            MessageBox.Show("Hotovo!");
        }

        private void FoodBackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            int pocet = (int)e.Argument;
            for (int i = pet.Food; i >= 0; i-- ) {
                if (foodClicked) {
                    pet.Food = 99;
                    i = pet.Food;                    
                    foodClicked = false;
                }
                pet.Food = i;
                Thread.Sleep(foodNeed);
                int procent = (int)Math.Round(100 * i / (double)100);
                foodBackgroundWorker.ReportProgress(procent);
            }
        }
        private void FoodBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            FoodProgress = e.ProgressPercentage;
            CheckHappy();
            MikPropertyChanged(nameof(FoodProgress));
        }
        private void FoodBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            MessageBox.Show("Hotovo!");
        }

        private void HygieneBackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            int pocet = (int)e.Argument;
            for (int i = pet.Hygiene; i >= 0; i-- ) {
                if (hygieneClicked) {
                    pet.Hygiene = 99;
                    i = pet.Hygiene;
                    hygieneClicked = false;
                }
                pet.Hygiene = i;
                Thread.Sleep(hygieneNeed);
                int procent = (int)Math.Round(100 * i / (double)100);
                hygieneBackgroundWorker.ReportProgress(procent);
            }
        }
        private void HygieneBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            HygieneProgress = e.ProgressPercentage;
            CheckHappy();
            MikPropertyChanged(nameof(HygieneProgress));
        }
        private void HygieneBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            MessageBox.Show("Hotovo!");
        }

        private void WalkBackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            int pocet = (int)e.Argument;
            for (int i = pet.Walk; i >= 0; i-- ) {
                if (walkClicked) {
                    pet.Walk = 99;
                    i = pet.Walk;
                    walkClicked = false;
                }
                pet.Walk = i;
                Thread.Sleep(walkNeed);
                int procent = (int)Math.Round(100 * i / (double)100);
                walkBackgroundWorker.ReportProgress(procent);
            }
        }
        private void WalkBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            WalkProgress = e.ProgressPercentage;
            CheckHappy();
            MikPropertyChanged(nameof(WalkProgress));
        }
        private void WalkBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            MessageBox.Show("Hotovo!");
        }

        public void MikPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public async void Sleep() {
            sleepClicked = true;
            await DoplnPotrebu("sleep");            
        }
        public async void Food() {
            foodClicked = true;
            await DoplnPotrebu("food");
        }
        public async void Walk() {
            walkClicked = true;
            await DoplnPotrebu("walk");
        }
        public async void Hygiene() {
            hygieneClicked = true;
            await DoplnPotrebu("hygiene");
        }
        public void CheckHappy() {
            bool aktMood = pet.IsHappy();
            if (aktMood != petMood && !delamPotrebu) {
                petMood = aktMood;
                NastavVychoziObrazek();
            }
        }
        private async Task DoplnPotrebu(string jaka) {
            delamPotrebu = true;
            if (jaka=="food") PetImage = "images/pet-food.png";
            if (jaka == "sleep") PetImage = "images/sleeping-pet.png";
            if (jaka == "walk") PetImage = "images/walking-pet.png";
            if (jaka == "hygiene") PetImage = "images/dog.png";

            MikPropertyChanged(nameof(PetImage));
            //AktualniObrazekMazlicka = obrazky[obrazek];
            // Čeká se 10 sekund na doplnění potřeby.
            await Task.Delay(TimeSpan.FromSeconds(2));
            NastavVychoziObrazek();
            delamPotrebu = false;
        }
        public void NastavVychoziObrazek() {
            if (!petMood) PetImage = "images/pet-unhappy.png";
            else PetImage = "images/pet.png";
            MikPropertyChanged(nameof(PetImage));
        }
    }
}

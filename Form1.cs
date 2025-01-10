using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace casello_autostradale
{
    public partial class Form1 : Form
    {
        // Costanti per i prezzi al chilometro
        const double prezzoNormale = 0.15; // Prezzo normale per chilometro
        const double prezzoTelepass = 0.10; // Prezzo Telepass per chilometro

        // Lista elementi grafici
        List<Corsia> corsie = new List<Corsia>();
        PictureBox[] pictureCorsie; // Array di PictureBox per rappresentare le auto
        Label[] lblPrezzoCorsie; // Array di Label per mostrare i prezzi
        Label[] lblDistanzaCorsie; // Array di Label per mostrare la distanza
        Panel[] pannelliCorsie; // Array di Pannelli per ogni corsia
        Panel[] lineeCorsie; // Linee verticali sui pannelli
        CancellationTokenSource[] tokenSources; // Array di token per gestire
                                                // l'annullamento dei thread

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Inizializza corsie con i rispettivi prezzi
            corsie.Add(new Corsia("Corsia 1", prezzoNormale));
            corsie.Add(new Corsia("Corsia 2", prezzoNormale));
            corsie.Add(new Corsia("Corsia 3", prezzoTelepass));
            corsie.Add(new Corsia("Corsia 4", prezzoTelepass));

            // Collega gli elementi grafici agli array elencati sopra
            pictureCorsie = new PictureBox[] { pbCorsia1, pbCorsia2, pbCorsia3, pbCorsia4 };
            lblPrezzoCorsie = new Label[] { lblPrezzo1, lblPrezzo2, lblPrezzo3, lblPrezzo4 };
            lblDistanzaCorsie = new Label[] { lblDistanza1, lblDistanza2, lblDistanza3, lblDistanza4 };
            pannelliCorsie = new Panel[] { pannelloCorsia1, pannelloCorsia2, pannelloCorsia3, pannelloCorsia4 };
            lineeCorsie = new Panel[corsie.Count];

            // Aggiungi linee verticali alla fine dei pannelli in ogni corsia
            for (int i = 0; i < pannelliCorsie.Length; i++)
            {
                lineeCorsie[i] = new Panel
                {
                    Size = new Size(5, pannelliCorsie[i].Height), // Larghezza 5px e altezza uguale al pannello
                    Location = new Point(pannelliCorsie[i].Width - 25, 0), // Posizionata alla fine del pannello
                    BackColor = Color.Red // Linea inizialmente rossa
                };
                pannelliCorsie[i].Controls.Add(lineeCorsie[i]); // Aggiunge la linea al pannello
            }

            // Inizializza token per gestire i thread
            tokenSources = new CancellationTokenSource[corsie.Count];
            for (int i = 0; i < tokenSources.Length; i++)
            {
                tokenSources[i] = new CancellationTokenSource();
            }

            // Avvia simulazione per ogni corsia
            for (int i = 0; i < corsie.Count; i++)
            {
                AvviaSimulazioneCorsia(i, corsie[i], pictureCorsie[i], lblPrezzoCorsie[i], lblDistanzaCorsie[i]);
            }
        }

        private void AvviaSimulazioneCorsia(int indice, Corsia corsia, PictureBox pbAuto, Label lblPrezzo, Label lblDistanza)
        {
            Task.Run(async () =>
            {
                Random rnd = new Random(); // Genera numeri casuali
                CancellationToken token = tokenSources[indice].Token; // Token per annullare il thread

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        // Simula arrivo di un'auto ogni 1-2 secondi
                        int delay = rnd.Next(1000, 2000);

                        // Aggiungi un'auto alla corsia con distanza casuale
                        double distanza = rnd.Next(10, 100);
                        corsia.AggiungiAuto(distanza); // Aggiunge l'auto alla corsia

                        // Mostra l'auto lungo la corsia simulando il movimento
                        this.Invoke(new Action(() => pbAuto.Visible = true));
                        int startX = 10; // Posizione iniziale
                        int endX = pannelliCorsie[indice].Width - 120; // Posizione finale sulla corsia
                        int step = 5; // Passo di movimento

                        for (int x = startX; x < endX; x += step)
                        {
                            if (token.IsCancellationRequested) return;

                            this.Invoke(new Action(() =>
                            {   // Aggiorna la posizione dell'auto
                                pbAuto.Location = new Point(x, pbAuto.Location.Y);
                            }));
                            await Task.Delay(50);
                        }

                        // Attendi 1 secondo
                        await Task.Delay(1000);

                        // Raggiunta la sbarra, mostra distanza e prezzo
                        this.Invoke(new Action(() =>
                        {
                            lblDistanza.Text = $"Distanza: {distanza} km";
                            lblDistanza.Visible = true;
                        }));

                        await Task.Delay(1000); // La label della distanza resta visibile per 4 secondi

                        double prezzo = distanza * corsia.PrezzoPerKm;
                        prezzo += prezzo * 22 / 100; // Aggiunge l'IVA

                        // Mostra prezzo calcolato
                        this.Invoke(new Action(() =>
                        {
                            lblPrezzo.Text = $"Prezzo: € {Math.Round(prezzo, 2)}";
                        }));

                        // Attendi 1 secondo
                        await Task.Delay(1000);

                        // La linea diventa verde
                        this.Invoke(new Action(() => lineeCorsie[indice].BackColor = Color.Green));
                        
                        // Attendi 1 secondo
                        await Task.Delay(1000);

                        // Reset dopo elaborazione
                        this.Invoke(new Action(() =>
                        {
                            lblPrezzo.Text = "Prezzo: € 0.00";
                            pbAuto.Visible = false;
                            lblDistanza.Visible = false;
                            lineeCorsie[indice].BackColor = Color.Red;
                        }));

                        corsia.RimuoviAuto(); // Rimuove l'auto dalla coda
                    }
                }
                catch (TaskCanceledException)
                {
                    // L'operazione è stata interrotta: gestire l'eccezione
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nella simulazione corsia {indice + 1}: {ex.Message}");
                }
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Interrompi tutti i thread al termine del programma
            foreach (var tokenSource in tokenSources)
            {
                tokenSource.Cancel();
            }
        }

        private void label1_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label8_Click(object sender, EventArgs e) { }
        private void label7_Click(object sender, EventArgs e) { }
    }
}

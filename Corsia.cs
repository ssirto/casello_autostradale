using System.Collections.Generic;

namespace casello_autostradale
{
    
    public class Corsia
    {
        // Coda che rappresenta le auto in attesa
        public Queue<double> CodaAuto { get; private set; } = new Queue<double>();

        // Prezzo per chilometro per corsia (normale o Telepass)
        public double PrezzoPerKm { get; set; }

        // Nome della corsia (es. "Corsia 1")
        public string NomeCorsia { get; set; }

        // Costruttore della classe Corsia
        // Inizializza il nome della corsia e il prezzo per chilometro
        public Corsia(string nome, double prezzoPerKm)
        {
            NomeCorsia = nome; // Imposta il nome della corsia
            PrezzoPerKm = prezzoPerKm; // Imposta il prezzo per chilometro
        }

        // Metodo per aggiungere un'auto alla coda
        public void AggiungiAuto(double distanza)
        {
            lock (CodaAuto) // Blocca l'accesso alla coda per garantire la sincronizzazione tra thread
            {
                CodaAuto.Enqueue(distanza); // Aggiunge la distanza dell'auto alla coda
            }
        }

        // Metodo per rimuovere un'auto dalla coda
        // Restituisce la distanza dell'auto rimossa o null se la coda è vuota
        public double? RimuoviAuto()
        {
            lock (CodaAuto) // Blocca l'accesso alla coda per garantire la sincronizzazione tra thread
            {
                if (CodaAuto.Count > 0) // Controlla se ci sono auto nella coda
                    return CodaAuto.Dequeue(); // Rimuove e restituisce la distanza dell'auto in testa alla coda
                return null; // Restituisce null se la coda è vuota
            }
        }
    }
}

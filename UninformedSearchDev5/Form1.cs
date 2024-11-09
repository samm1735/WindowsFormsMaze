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



////
///     Nom: ISAAC Sammuel Ramclief
///     Cours: Introduction à l'intelligence artificielle
///     Devoir: Devoir 5
///             -> Maze Uninformed Search
///


namespace UninformedSearchDev5
{
    public partial class Form1 : Form
    {
        bool FirstPaintEvent = true;
        int NombreDeLignes, NombreDeColomnes;
        int offsetX = 40, offsetY = 20;

        Node Depart, Destination;
        private Node[][] Positions; // Corpus - Population
        private List<Node> ListeSolution;
        List<Node> NodesExplorees;

        /// <summary>
        /// Constructeur de la classe Form1
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Méthode appelée lors du premier affichage du formulaire.
        ///  Si FirstPaintEvent est vrai , la méthode appelle Setup().
        /// </summary>
        /// <param name="e">Un event</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (FirstPaintEvent)
            {
                Setup();
            }
        }

        /// <summary>
        /// Méthode appelée lorsque la taille du formulaire change.
        /// </summary>
        /// <param name="e">Un event</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Setup();
        }

        /// <summary>
        /// Maximise la fenêtre, ajuste la taille du bouton btnStart et du pictureBox2 pour occuper la moitié de la largeur de la fenêtre.
        /// </summary>
        private void Setup()
        {
            FirstPaintEvent = false;
            WindowState = FormWindowState.Maximized;
            btnStart.Height = 30;
            btnStart.Width = 50;
            //pictureBox1.Width = -40 + ClientSize.Width / 2;
            pictureBox2.Width = -30 + ClientSize.Width / 2;
        }

        /// <summary>
        /// Méthode de chargement du formulaire,
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Classe pour representer une cellule du maze
        /// </summary>
        public class Node
        {
            public int x = -1, y = -1;
            public bool EstMur = false;
            public Node parent = null;

            /// <summary>
            /// Constructoeur d'un Node avec les coordonnées et le parent spécifié.
            /// </summary>
            /// <param name="x_val"></param>
            /// <param name="y_val"></param>
            /// <param name="parent"></param>
            public Node(int x_val, int y_val, Node parent)
            {
                x = x_val;
                y = y_val;
                this.parent = parent;
            }
        }


        /// <summary>
        /// Déclenche la résolution du maze lorsqu'on clique sur le bouton.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (LireFichier())
            {
                Solve();
                Imprimer();
            }
        }

        /// <summary>
        /// Lit le fichier du labyrinthe et initialise les nœuds
        /// Ouvre le fichier maze6.txt , lit le contenu, et initialise chaque cellule du tableau Positions comme un objet Node . 
        /// Marque les murs, les paths, et les positions de départ et destination(A pour le départ, B pour la destination).
        /// </summary>
        /// <returns></returns>
        private bool LireFichier()
        {
            Depart = new Node(0, 0, null);
            Destination = new Node(0, 0, null);

            FileInfo fichier = new FileInfo("../../maze6.txt");
            if (!fichier.Exists)
            {
                return false;
            }
            StreamReader lecture = fichier.OpenText();
            string tout = lecture.ReadToEnd();

            string[] lines = null;
            if (tout.Contains('\n') && tout.Contains('\r'))
            {
                lines = tout.Replace("\n", "").Split('\r');
            }
            else if (tout.Contains('\n'))
            {
                lines = tout.Split('\n');
            }
            else if (tout.Contains('\r'))
            {
                lines = tout.Split('\r');
            }
            if (lines == null)
                return false;
            NombreDeLignes = lines.Length;
            NombreDeColomnes = lines[0].Length;


            // initialiser Positions
            Positions = new Node[NombreDeLignes][];
            for (int i = 0; i < NombreDeLignes; i++)
            {
                Positions[i] = new Node[NombreDeColomnes];
                for (int j = 0; j < NombreDeColomnes; j++)
                {
                    Positions[i][j] = new Node(i, j, null);
                    switch (lines[i][j])
                    {
                        case 'A':
                            Depart.x = i; Depart.y = j;
                            break;
                        case 'B':
                            Destination.x = i; Destination.y = j;
                            break;
                        case '#':
                            Positions[i][j].EstMur = true;
                            break;
                        case ' ':
                            break;
                        default:
                            break;
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// Dessine le labyrinthe sur le PictureBoz -> pictureBox2 .
        /// </summary>
        public void Imprimer()
        {
            int cellSize = 20; // Size of each cell on the grid
            using (Graphics g = pictureBox2.CreateGraphics())
            {
                for (int i = 0; i < NombreDeLignes; i++)
                {
                    for (int j = 0; j < NombreDeColomnes; j++)
                    {
                        Brush brush;

                        if (i == Depart.x && j == Depart.y)
                            brush = Brushes.Green; // Start
                        else if (i == Destination.x && j == Destination.y)
                            brush = Brushes.Red; // Destination                        
                        else if (Positions[i][j].EstMur)
                            brush = Brushes.Black; // Wall
                        else
                            brush = Brushes.White; // Path

                        // If solved, draw the solution path
                        if (ListeSolution != null && ListeSolution.Any(n => n.x == i && n.y == j))
                        {
                            if (!(i == Depart.x && j == Depart.y || i == Destination.x && j == Destination.y))
                            {
                                brush = Brushes.Yellow;
                            }
                            
                        }

                        // Draw cell
                        g.FillRectangle(brush, offsetX + j * cellSize, offsetY + i * cellSize, cellSize, cellSize);
                        g.DrawRectangle(Pens.Gray, offsetX + j * cellSize, offsetY + i * cellSize, cellSize, cellSize);
                    }
                }
            }
        }


        /// <summary>
        /// Résout le labyrinthe en utilisant une recherche en profondeur
        /// </summary>
        private void Solve()
        {
            // Stack: trouvera la route la plus courte
            NodesExplorees = new List<Node>();
            StackFrontier StackOuQueue = new StackFrontier();
            StackOuQueue.Ajouter(Depart);
            Node node = null;

            while (!StackOuQueue.EstVide())
            {
                node = StackOuQueue.Enlever();
                NodesExplorees.Add(node);
                if (node.x == 8 && node.y == 13)
                {
                    int i;
                    i = 0;
                }
                // Si le noeud est le noeud de destination, alors terminer
                if (node.x == Destination.x && node.y == Destination.y)
                {
                    // Construire le chemin de la destination à la source: Solution
                    ListeSolution = new List<Node>();
                    while (node != null)
                    {
                        ListeSolution.Add(node);
                        node = node.parent;
                    }
                    break;
                }
                // Ajouter les enfants Node
                List<Node> enfants = ListeDesEnfants(node);
                foreach (Node n in enfants)
                {
                    if (!StackOuQueue.ContientState(n) && !DejaExploree(n))
                    {
                        StackOuQueue.Ajouter(n);
                    }
                }

            }
        }

        /// <summary>
        /// Génère une liste des nœuds enfants (voisins accessibles) pour un nœud donné
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        List<Node> ListeDesEnfants(Node node)
        {
            List<Node> nodesActions = new List<Node>();
            if ((node.x - 1 >= 0 && node.x - 1 < NombreDeLignes) &&
                (node.y >= 0 && node.y < NombreDeColomnes))
            {
                Node n = new Node(node.x - 1, node.y, node);
                if (!Positions[n.x][n.y].EstMur)
                {
                    nodesActions.Add(n);
                }
            }
            if ((node.x + 1 >= 0 && node.x + 1 < NombreDeLignes) &&
                (node.y >= 0 && node.y < NombreDeColomnes))
            {
                Node n = new Node(node.x + 1, node.y, node);
                if (!Positions[n.x][n.y].EstMur)
                {
                    nodesActions.Add(n);
                }
            }
            if ((node.x >= 0 && node.x < NombreDeLignes) &&
                (node.y - 1 >= 0 && node.y - 1 < NombreDeColomnes))
            {
                Node n = new Node(node.x, node.y - 1, node);
                if (!Positions[n.x][n.y].EstMur)
                {
                    nodesActions.Add(n);
                }
            }
            if ((node.x >= 0 && node.x < NombreDeLignes) &&
                (node.y + 1 >= 0 && node.y + 1 < NombreDeColomnes))
            {
                Node n = new Node(node.x, node.y + 1, node);
                if (!Positions[n.x][n.y].EstMur)
                {
                    nodesActions.Add(n);
                }
            }
            return nodesActions;
        }


        /// <summary>
        /// Vérifie si un nœud a déjà été exploré
        /// </summary>
        /// <param name="aChequer"></param>
        /// <returns></returns>
        public bool DejaExploree(Node aChequer)
        {
            foreach (Node n in NodesExplorees)
            {
                if (n.x == aChequer.x && n.y == aChequer.y)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Représente une pile pour la recherche en profondeur.
        /// </summary>
        public class StackFrontier
        {
            private List<Node> ListeDesNodes;

            /// <summary>
            /// Constructeur de la pile
            /// </summary>
            public StackFrontier()
            {
                ListeDesNodes = new List<Node>();
            }

            /// <summary>
            /// Verifie si lala pile est vide
            /// </summary>
            /// <returns></returns>
            public bool EstVide()
            {
                return ListeDesNodes.Count == 0;
            }

            /// <summary>
            /// Vérifie si un nœud spécifique est dans la pile
            /// </summary>
            /// <param name="node">Un node</param>
            /// <returns>True si la liste est vide</returns>
            public bool ContientState(Node node)
            {
                foreach (Node n in ListeDesNodes)
                {
                    if (n.x == node.x && n.y == node.y)
                        return true;
                }
                return false;
            }
            /// <summary>
            /// Adds an object to the beginning of the List<T>. LIFO
            /// </summary>
            /// <param name="node"></param>
            public void Ajouter(Node node)
            {
                ListeDesNodes.Add(node);
            }
            /// <summary>
            /// Pops an object from the beginning of the List<T>
            /// </summary>
            /// <returns></returns>
            public Node Enlever()
            {
                Node node = ListeDesNodes[ListeDesNodes.Count - 1];
                ListeDesNodes.RemoveAt(ListeDesNodes.Count - 1);
                return node;
            }
        }

        /// <summary>
        /// Représente une file pour la recherche en profondeur.
        /// </summary>
        public class QueueFrontier
        {
            private List<Node> ListeDesNodes;

            /// <summary>
            /// Constructeur qui initialise une liste vide pour les nœuds.
            /// </summary>
            public QueueFrontier()
            {
                ListeDesNodes = new List<Node>();
            }

            /// <summary>
            /// Vérifie si la file est vide.
            /// </summary>
            /// <returns></returns>
            public bool EstVide()
            {
                return ListeDesNodes.Count == 0;
            }

            /// <summary>
            /// Vérifie si un nœud spécifique est dans la file.
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            public bool ContientState(Node node)
            {
                foreach (Node n in ListeDesNodes)
                {
                    if (n.x == node.x && n.y == node.y)
                        return true;
                }
                return false;
            }
            /// <summary>
            /// Pushes an item to the end of the List<T>
            /// </summary>
            /// <param name="node"></param>
            public void Ajouter(Node node)
            {
                // Add: Adds an object to the beginning of the List<T>.
                ListeDesNodes.Insert(0, node);
            }
            /// <summary>
            /// Pops an item from the beginning of the List<T>
            /// </summary>
            /// <returns></returns>
            public Node Enlever()
            {
                Node node = ListeDesNodes[ListeDesNodes.Count - 1];
                ListeDesNodes.RemoveAt(ListeDesNodes.Count - 1);
                return node;
            }
        }
    }
}

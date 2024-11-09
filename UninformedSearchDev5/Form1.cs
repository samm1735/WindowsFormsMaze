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
        public Form1()
        {
            InitializeComponent();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (FirstPaintEvent)
            {
                Setup();
            }
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Setup();
        }
        private void Setup()
        {
            FirstPaintEvent = false;
            WindowState = FormWindowState.Maximized;
            btnStart.Height = 30;
            btnStart.Width = 50;
            //pictureBox1.Width = -40 + ClientSize.Width / 2;
            pictureBox2.Width = -30 + ClientSize.Width / 2;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public class Node
        {
            public int x = -1, y = -1;
            public bool EstMur = false;
            public Node parent = null;
            public Node(int x_val, int y_val, Node parent)
            {
                x = x_val;
                y = y_val;
                this.parent = parent;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (LireFichier())
            {
                Solve();
                Imprimer();
            }
        }

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
        
        public void Imprimer()
        {
            int cellSize = 20; // Size of each cell on the grid
            bool isCaseDeDepart = false, isCaseDeDestination = false;
            using (Graphics g = pictureBox2.CreateGraphics())
            {
                for (int i = 0; i < NombreDeLignes; i++)
                {
                    for (int j = 0; j < NombreDeColomnes; j++)
                    {
                        Brush brush;

                        if (i == Depart.x && j == Depart.y)
                        {
                            isCaseDeDepart = true;
                            brush = Brushes.Green; // Start
                        }
                        else if (i == Destination.x && j == Destination.y)
                        {
                            isCaseDeDestination = true;
                            brush = Brushes.Red; // Destination
                        }
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

        private void pictureBox2_Click(object sender, EventArgs e)
        {
        }

        public bool DejaExploree(Node aChequer)
        {
            foreach (Node n in NodesExplorees)
            {
                if (n.x == aChequer.x && n.y == aChequer.y)
                    return true;
            }
            return false;
        }
        public class StackFrontier
        {
            private List<Node> ListeDesNodes;
            public StackFrontier()
            {
                ListeDesNodes = new List<Node>();
            }
            public bool EstVide()
            {
                return ListeDesNodes.Count == 0;
            }
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
        public class QueueFrontier
        {
            private List<Node> ListeDesNodes;
            public QueueFrontier()
            {
                ListeDesNodes = new List<Node>();
            }
            public bool EstVide()
            {
                return ListeDesNodes.Count == 0;
            }
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

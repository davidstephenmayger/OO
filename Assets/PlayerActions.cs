using UnityEngine;
using System.Collections;
namespace PlayerActions {
    public interface IFloor
    {
        IMover GetPlayer();
        void SetPlayer(IMover player);
        void SetBoard(ISquare[,] board);
        ISquare[,] GetBoard();
    }
    public interface IPlayerAction
    {
        void ResolveAction(IFloor floor);
        char GetChar();
    }
    public struct Tuple<A, B>
        where A : struct
        where B : struct
    {
        public A x {get; set;}
        public B y { get; set; }
        // the "this()" thinaging is to do with the automatic property, the code doesn't like using the setter to set the property. REF solution: http://stackoverflow.com/questions/2534960/struct-constructor-fields-must-be-fully-assigned-before-control-is-returned-to#2534974
        public Tuple(A first, B second) : this()
        {
            this.x = first;
            this.y = second;
        }
    }
    public interface ISquare : IGetSquare, ISetSquare
    {
    }
    public interface IGetSquare
    {
        ITile GetTile();
        IMover GetMover();
    }
    public interface ISetSquare
    {
        void SetMover(IMover m);
    }
    public interface ITile
    {
        bool AllowsMove();    //true if monster allowed, false if not.
    }
    public interface IMover : IAttack, IHealth
    {
    }
    public interface IHealth
    {
        void TakeAttack(IIncomingDamage damage);
        bool IsAlive();
        int GetMaxHealth();
    }
    public interface IDefend
    {
        IIncomingDamage Reduce(IIncomingDamage incoming);
    }
    public interface IIncomingDamage
    {
        int GetDamage();
        void SetDamage(int d);
    }
    public interface IAttack
    {
        IIncomingDamage PerformAttack();
    }
    static class UtilFuncs
    {
        public static System.Random r = new System.Random();
        public static IIncomingDamage Dodge(IIncomingDamage incoming, int dodgePercent)
        {
            int dodgeRoll = r.Next(0, 100);
            if (dodgeRoll < dodgePercent)
                incoming.SetDamage(0);
            return incoming;
        }
        public static IIncomingDamage Armour(IIncomingDamage incoming, int armor)
        {
            incoming.SetDamage(incoming.GetDamage() - r.Next(0,armor));
            return incoming;
        }
        /// <summary>
        /// Whether or not a position is within the arrays bounds.
        /// </summary>
        /// <typeparam name="T">any old type</typeparam>
        /// <param name="array">the array who's bounds were checking x and y are in</param>
        /// <param name="x">position on first axis to check</param>
        /// <param name="y">position on second axis to check</param>
        /// <returns></returns>
        public static bool InBounds<T>(T[,] array, int x, int y)
        {
            int w = array.GetLength(0);
            int h = array.GetLength(1);
            if (x >= 0 && x < w && y >= 0 && y < h)
                return true;
            return false;
        }
        /// <summary>
        /// Determines whether a board meets the criteria of being 'valid'
        /// 1. not being null.
        /// 2. Having no null square elements in its array.
        /// 3. none of those square elements having any null tiles.
        /// </summary>
        /// <typeparam name="T">a type that is an ISquare</typeparam>
        /// <param name="board">an array of that type</param>
        /// <returns></returns>
        public static bool IsValidBoard(ISquare[,] board)
        {
            //if board is null, is invalid.
            if (board == null)
                return false;

            //if any square of board
            for (int i = 0; i < board.GetLength(0); i++)
                for (int j = 0; j < board.GetLength(1); j++)
                    //is null, is invalid
                    if (board[i, j] == null)
                    {
                        return false;
                    }
                    //has a null tile, is invalid.
                    else
                    {
                        if (board[i, j].GetTile() == null)
                            return false;
                    }
            //if all of that isn't the case, board valid.
            return true;
        }
        /// <summary>
        /// Moves the mover at position x,y, by dislocation xMove, yMove, if a valid place to move to is available.
        /// if invalid move for whatever reason returns without morphing board.
        /// This function changes board's internal state.
        /// </summary>
        /// <typeparam name="M">a mover</typeparam>
        /// <param name="board">a board</param>
        /// <param name="x">the x position to move the mover from</param>
        /// <param name="y">the y position to move the mover from</param>
        /// <param name=""xMove>how much to move it on the x axis.</param>
        /// <param name="yMove">how much to move it on the y axis</param>
        public static void Move(ISquare[,] board,int xPos, int yPos, int xMove, int yMove) 
        {
            //if move is no move, we don't have to do any work, return.
            if (xPos == 0 && yPos == 0)
                return;
            //if the board we are moving to is not in a valid state, return.
            if (!IsValidBoard(board))
                return;
            //if position supplied out of bounds, return
            if(!InBounds(board,xPos,yPos))
                return;
            //if the translated position we will be moving to is out of bounds, return.
            if (!InBounds(board, xPos, yPos))
                return;
            int newX = xPos + xMove;
            int newY = yPos + yMove;
            //if we have no monster to move to, return.
            if (board[xPos, yPos].GetMover() == null)
                return;
            //if the place we are moving to already has a monster, we instead perform an attack action.
            if (board[newX, newY].GetMover() != null)
            {
                Attack(board[xPos, yPos].GetMover(), board[newX, newY].GetMover());
                return;
            }
            //if the position we are moving to doesn't allow monsters, we return.
            if (!board[newX, newY].GetTile().AllowsMove())
                return;

            //main logic.
            IMover mover = board[xPos, yPos].GetMover();
            board[newX, newY].SetMover(mover);
            board[xPos, yPos].SetMover(null);
        }
        public static void TakeTurn(IFloor floor, char action)
        {
            IPlayerAction[] actions = GetAllActions();
            int w = actions.GetLength(0);
            for (int i = 0; i < w; i++)
                if (actions[i].GetChar() == action)
                {
                    TakeTurn(floor, actions[i]);
                    break;
                }
        }
        /// <summary>
        /// Resolves a turn, given the action the player took.
        /// </summary>
        /// <typeparam name="F"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <param name="floor"></param>
        /// <param name="takeAction"></param>
        public static void TakeTurn(IFloor floor, IPlayerAction takeAction)
        {
            //first we resolve the action.
            takeAction.ResolveAction(floor);

            //then we resolve the rest of the board. which in this case is nothing!
        }
        /// <summary>
        /// tries to find the player
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <param name="board">the board to search for hte player on.</param>
        /// <returns>-9,-9 if player hasn't been found, x,y coord location if is found</returns>
        public static Tuple<int, int> FindPlayer(ISquare[,] board, IMover player)
        {
            int w = board.GetLength(0);
            int h = board.GetLength(1);
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                    if (board[i, j] != null)
                        if (board[i, j].GetMover() != null)
                            if (board[i, j].GetMover().Equals(player))
                                return new Tuple<int, int>(i, j);
            return new Tuple<int,int>(-1,-1);
        }
        public static void Attack(IMover attacker, IMover defender)
        {
            if (attacker == null || defender == null)
                return;
            defender.TakeAttack(attacker.PerformAttack());
        }
        private static IPlayerAction[] actions;
        private static IPlayerAction[] GetAllActions()
            {
                if (actions != null)
                    return actions;
                actions = new IPlayerAction[9];
                char c = 'a';
                for(int i = 0; i < 9;i++)
                {
                    int x = i % 3;
                    int y = i / 3;
                    actions[i] = new MoveAction(new Tuple<int,int>(x,y),c);
                    c++;
                }
                return actions;
            }
        private class MoveAction : IPlayerAction
            {
                private Tuple<int, int> move;
                private char c;
                public MoveAction(Tuple<int,int> move,char c)
                {
                    this.c = c;
                    this.move = move;
                }
                public void ResolveAction(IFloor floor)
                {
                    ISquare[,] board = floor.GetBoard();
                    IMover player = floor.GetPlayer();
                    Tuple<int, int> location = FindPlayer(board, player);
                    Move(board, location.x, location.y, move.x, move.y);
                }
                public char GetChar()
                {
                    return c;
                }

            }
    }
    
    public static class Factory
    {
        /// <summary>
        /// Returns a simple board.
        /// </summary>
        /// <returns></returns>
        public static IFloor MakeSimpleFloor()
        {
            int l = 5;
            ISquare[,] board = new ISquare[l, l];
            for (int i = 0; i < l; i++)
                for (int j = 0; j < l; j++)
                    if (i == 0 || i + 1 == l || j == 0 || j + 1 == l)
                        board[i, j] = WrapTile(MakeTileWall());
                    else
                        board[i, j] = WrapTile(MakeTileEmpty());

            IMover player = MakeBasicMover(3,9);
            board[2, 2].SetMover(player);
            return new Floor(board, player);
        }
        private static ISquare WrapTile(ITile t)
        {
            return new Square(t,null);
        }
        public static ITile MakeTileEmpty()
        {
            return new Tile(true);
        }
        public static ITile MakeTileWall()
        {
            return new Tile(false);
        }
        public static IMover MakeBasicMover(int damage,int health)
        {
            IAttack attack = new Attack(damage, damage);
            IHealth h = new Health(3, new IDefend[0]);
            return new Mover(attack,h);
        }
        private class Mover : IMover
        {
            IAttack attack;
            IHealth health;
            public Mover(IAttack attack, IHealth health)
            {
                this.attack = attack;
                this.health = health;
            }
            public void TakeAttack(IIncomingDamage damage)
            {
                health.TakeAttack(damage);
            }
            public bool IsAlive()
            {
                return health.IsAlive();
            }
            public int GetMaxHealth()
            {
                return health.GetMaxHealth();
            }
            public IIncomingDamage PerformAttack()
            {
                return attack.PerformAttack();
            }
        }
        private class Square : ISquare
        {
            ITile t;
            IMover m;

            public Square(ITile t, IMover m)
            {
                this.t = t;
                this.m = m;
            }
            public ITile GetTile()
            {
                return t;
            }
            public IMover GetMover()
            {
                return m;
            }
            public void SetMover(IMover m)
            {
                this.m = m;
            }
        }
        private class Floor : IFloor
        {
            ISquare[,] board;
            IMover player;

            public Floor(ISquare[,] board, IMover player)
            {
                this.board = board;
                this.player = player;
            }
            public IMover GetPlayer()
            {
                return player;
            }
            public void SetPlayer(IMover player)
            {
                this.player = player;
            }
            public void SetBoard(ISquare[,] board)
            {
                this.board = board;
            }
            public ISquare[,] GetBoard()
            {
                return board;
            }
        }
        private class Tile : ITile
        {
            bool allowsMove;
            public Tile(bool allowsMove)
            {
                this.allowsMove = allowsMove;
            }
            public bool AllowsMove()
            {
                return allowsMove;
            }
        }
        private class Health : IHealth
        {
            int health;
            int startingHealth;
            public Health(int startingHealth, IDefend[] defences)
            {
                this.health = startingHealth;
                this.startingHealth = startingHealth;
            }
            public void SetHealth(int health)
            {
                this.startingHealth = health;
            }
            public int GetHealth()
            {
                return health;
            }
            public int GetMaxHealth()
            {
                return startingHealth;
            }
            public void TakeAttack(IIncomingDamage damage)
            {
                health = health - damage.GetDamage();
            }
            public bool IsAlive()
            {
                if (health > 0) 
                    return true;
                return false;
            }
        }
        private class Attack : IAttack
        {
            int damageMin;
            int damageMax;
            public Attack(int damageMin,int damageMax)
            {
                this.damageMin = damageMin;
                this.damageMax = damageMax;
            }
            public IIncomingDamage PerformAttack()
            {
                return new IncomingDamage(UtilFuncs.r.Next(damageMin, damageMax));
            }
        }
        private class IncomingDamage : IIncomingDamage
        {
            int damage;
            public IncomingDamage(int damage)
            {
                this.damage = damage;
            }
            public int GetDamage()
            {
                return damage;
            }
            public void SetDamage(int d)
            {
                damage = d;
            }
        }
    }
}

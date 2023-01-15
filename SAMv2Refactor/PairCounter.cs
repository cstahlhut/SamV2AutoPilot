namespace IngameScript
{
    partial class Program
    {
        private class PairCounter
        { // PairCounter
            public int oldCounter;
            public int newCounter;
            public PairCounter()
            {
                this.oldCounter = 0;
                this.newCounter = 1;
            }

            public void Recount()
            {
                this.oldCounter = this.newCounter;
                this.newCounter = 0;
            }

            public int Diff()
            {
                return newCounter - oldCounter;
            }
        }
    }
}

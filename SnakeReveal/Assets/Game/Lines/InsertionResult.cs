namespace Game.Lines
{
    public readonly struct InsertionResult
    {
        /// <summary>
        ///     The (maybe new) line that the chain flows into after the reconnection point
        /// </summary>
        public readonly Line Continuation;

        /// <summary>
        ///     Whether the line chain reconnected to this loop in the turn of this loop
        /// </summary>
        public readonly bool IsStartToEnd;

        public InsertionResult(Line continuation, bool isStartToEnd)
        {
            Continuation = continuation;
            IsStartToEnd = isStartToEnd;
        }
    }
}
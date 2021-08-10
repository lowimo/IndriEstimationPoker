using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace IndriEstimationPoker
{
    public class EstimationStateRepo
    {
        private readonly Dictionary<string, EstimationState> states = new Dictionary<string, EstimationState>();

        public void Create( string id )
        {
            lock ( this )
            {
                states[id] = new EstimationState();
                List<string> expires = states
                    .Where( kvp => ( DateTime.UtcNow - kvp.Value.CreatedUtc ) > TimeSpan.FromMinutes( 10 ) && kvp.Value.Players.Count == 0 )
                    .Select( kvp => kvp.Key )
                    .ToList();

                foreach ( var item in expires )
                {
                    states.Remove( item );
                }
            }
        }

        public EstimationState GetOrDefault( string id )
        {
            return states.GetValueOrDefault( id );
        }
    }

    public class EstimationState
    {
        public DateTime CreatedUtc { get; }
        public long Version { get; private set; }
        public ImmutableDictionary<string, Player> Players { get; private set; } = ImmutableDictionary<string, Player>.Empty;
        public bool IsRevealed { get; set; }

        public EstimationState()
        {
            CreatedUtc = DateTime.UtcNow;
        }

        public Player CreatePlayer( string id, string name )
        {
            var player = new Player( id, name );
            Mutate( x => x.Players = x.Players.SetItem( id, player ) );
            return player;
        }

        public void RemovePlayer(Player p)
        {
            Mutate( x => x.Players = x.Players.Remove( p.ID ) );
        }

        public void Mutate( Action<EstimationState> mutate )
        {
            lock( this )
            {
                mutate( this );
                Version++;
            }
        }
    }

    public class Player
    {
        public string ID { get; }
        public string Name { get; set; }
        public int? EstimationValue { get; set; }

        public Player( string id, string name )
        {
            ID = id;
            Name = name;
        }
    }
}
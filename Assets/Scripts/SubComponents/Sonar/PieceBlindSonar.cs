using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PieceBlindSonar : Sonar
{
    protected override void Ping()
    {
        var boundary = new SortedSet<(Vector3Int position, int quotient)>(new PositionComparer());

        foreach ((Vector3Int position, int quotient) item in
            Parent
            .GetBody()
            .Select(v => (v, div: ScalarDivision(v, direction)))
            .GroupBy(x => x.div.remainder)
            .Select(g => g.OrderByDescending(x => x.div.quotient).First())
            .Select(g => (g.v, g.div.quotient)))
            boundary.Add(item);

        List<int> distances = new();
        HashSet<GameObject> collideds = new();
        int valid = 0;
        foreach (var (pos, quotient) in boundary)
            for (int i = 0; i < maxDistance; i++)
                if (World.CheckCollision(pos + (i + 1) * direction, out CoreComponent collided) && collided is not Piece)
                {
                    distances.Add(i);
                    collideds.Add(collided.gameObject);
                    valid++;
                    break;
                }

        clearance.Value = valid switch
        {
            0 => boundary.Select(p => -p.quotient).ToArray(),
            var n when n == boundary.Count => distances.ToArray(),
            _ => Enumerable.Repeat(-1, boundary.Count).ToArray()
        };
        Valid = valid == boundary.Count;
        collidedObjects.Value = collideds.ToArray();
    }
}
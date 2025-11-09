using System.Collections.Generic;
using System.Linq;


public class LeaderboardManager
{
    List<Entity> _tracked = new List<Entity>();


    public void Register(Entity e)
    {
        if (!_tracked.Contains(e)) _tracked.Add(e);
    }


    public void Unregister(Entity e) => _tracked.Remove(e);


    public List<Entity> Top(int count = 8)
    => _tracked.OrderByDescending(e => e.stats.mass).Take(count).ToList();
}
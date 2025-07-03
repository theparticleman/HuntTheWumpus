namespace HuntTheWumpus.Domain.Entities;

public class Player
{
    public Room CurrentRoom { get; private set; }
    public int ArrowCount { get; private set; }

    public Player(Room startingRoom)
    {
        CurrentRoom = startingRoom;
        ArrowCount = 5;
    }

    public void MoveTo(Room room)
    {
        CurrentRoom = room;
    }

    public void UseArrow()
    {
        if (ArrowCount > 0)
            ArrowCount--;
    }
}

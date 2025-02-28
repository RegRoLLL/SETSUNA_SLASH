using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnotherRoomManager : MonoBehaviour
{
    [SerializeField] List<StagePart> anotherRooms = new();

    public List<string> GetNameList() => anotherRooms.Select(room => room.GetTitle()).ToList();
}

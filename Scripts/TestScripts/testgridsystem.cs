using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testgridsystem : MonoBehaviour
{
    [SerializeField]
    private Grid grid;
    [SerializeField]
    private GameObject tileprefab;
    [SerializeField]
    Vector3Int position;
    Vector3 localPosition;


    private void Start()
    {
        grid.transform.position = transform.position;
        localPosition = Vector3.zero;
        position = Vector3Int.zero;
        // Instantiate the first tile at the current position
        InstantiateTile();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int xoffset = 0;
            int yoffset = 0;
            if (Random.Range(0, 10) > 5)
            {
                xoffset = 1;
            }
            else
            {
                yoffset = 1;
            }

            // Update the position with offsets
            position = new Vector3Int(position.x + xoffset, position.y + yoffset, 0);


            // Instantiate the tile at the updated position
            InstantiateTile();
        }
    }

    private void InstantiateTile()
    {
        Vector3 worldPosition = grid.GetCellCenterWorld(position);
        GameObject newTile = Instantiate(tileprefab, worldPosition, Quaternion.identity,transform);

        // Set the localPosition of the tile within the panel's coordinate space
        //newTile.transform.localPosition = localPosition; // Set the local position
        newTile.transform.localScale = Vector3.one;
    }
}

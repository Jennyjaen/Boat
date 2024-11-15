using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDelimeter {
    int x { get; set; }
    int y { get; set; }
    int stream { get; set; }
    int zerostream { get; set; }
    int zerostream_x { get; set; }
    int zerostream_y { get; set; }
    int sum_x { get; set; }
    int sum_y { get; set; }
    int save_x { get; }
    int save_y { get; }
}

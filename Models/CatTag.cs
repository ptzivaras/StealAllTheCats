using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StealAllTheCats.Models;

public class CatTag
{
    public int CatEntityId { get; set; }
    public CatEntity CatEntity { get; set; }

    public int TagEntityId { get; set; }
    public TagEntity TagEntity { get; set; }
}
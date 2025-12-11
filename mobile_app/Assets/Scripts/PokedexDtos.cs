[System.Serializable]
public class MonsterPokedexDtoUnity
{
    public int monsterId;
    public string name;
    public string type1;
    public string type2;
    public string spriteUrl;
    public bool isHunted;
}

[System.Serializable]
public class PagedMonsterDtoUnity
{
    public int totalCount;
    public MonsterPokedexDtoUnity[] items;
}

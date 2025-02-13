using System.Collections.Generic;
using System.IO;
using Godot;
using Newtonsoft.Json;
public class LocalDatabase {
    protected readonly Dictionary<string, UnitDTO> units = new();
    protected readonly Dictionary<string, AbilityDTO> abilites = new();
    protected readonly Dictionary<string, EffectBaseDTO> effects = new();
    protected readonly Dictionary<string, MutationDTO> unitMutators = new();

    protected JSONLoader loader = new();

    public JSONLoader Loader { get => loader; }


    public Dictionary<string, UnitDTO> Units => units;
    public Dictionary<string, AbilityDTO> Abilites => abilites;
    public Dictionary<string, EffectBaseDTO> Effects => effects;
    public Dictionary<string, MutationDTO> UnitMutators => unitMutators;

    public void LoadData() {
        LoadUnits().ForEach(unit => units.Add(unit.name, unit));
        LoadAbilities().ForEach(ability => abilites.Add(ability.name, ability));
        LoadEffects().ForEach(effect => effects.Add(effect.id, effect));
        LoadMutators().ForEach(mutator => unitMutators.Add(mutator.id, mutator));
    }

    public EffectBaseDTO GetEffect(string key) {
        return effects[key];
    }

    public Dictionary<string, T> Load<T>(string name) where T : DTOBase {
        Dictionary<string, T> values = new();
        loader.GetListFromJson<T>(name).ForEach(value => values.Add(value.id, value));
        return values;
    }

    public List<UnitDTO> LoadUnits() {
        return loader.GetListFromJson<UnitDTO>("Units");
    }
    public List<AbilityDTO> LoadAbilities() {
        return loader.GetListFromJson<AbilityDTO>("Abilities");
    }
    public List<EffectBaseDTO> LoadEffects() {
        return loader.GetListFromJson<EffectBaseDTO>("Effects");
    }
    public List<MutationDTO> LoadMutators() {
        return loader.GetListFromJson<MutationDTO>("Mutators");
    }
}

public class JSONLoader {
    string databasePath = "./Database/";

    public JSONLoader() { }
    public JSONLoader(string databasePath) {
        this.databasePath = databasePath;
    }

    string LoadJson(string name) {
        using StreamReader reader = new(databasePath + name + ".json");
        string json = reader.ReadToEnd();
        return json;
    }

    public List<T> GetListFromJson<T>(string name) {
        string json = LoadJson(name);
        List<T> items = JsonConvert.DeserializeObject<List<T>>(json);
        return items;
    }

    public static Dictionary<string, T> Deserialize<T>(string data) {
        Dictionary<string, T> items = JsonConvert.DeserializeObject<Dictionary<string, T>>(data);
        return items;
    }

    public void UpdateDBPath(string newPath) {
        GD.Print("[UpdateDBPath] New path is " + newPath);
        databasePath = newPath;
    }

    public static T ForceToType<T>(object obj) {
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
    }
}

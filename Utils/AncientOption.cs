using MegaCrit.Sts2.Core.Models;

namespace BaseLib.Utils;

public abstract class AncientOption(int weight) : IWeighted
{
    public int Weight { get; } = weight;
    
    /// <summary>
    /// For special options like Orobas SeaGlass with multiple variants.
    /// </summary>
    public abstract IEnumerable<RelicModel> AllVariants { get; }
    public abstract RelicModel ModelForOption { get; }
    
    public static explicit operator AncientOption(RelicModel model) => new BasicAncientOption(model, 1);
    
    private class BasicAncientOption(RelicModel model, int weight) : AncientOption(weight)
    {
        public override IEnumerable<RelicModel> AllVariants { get; } = [ model.ToMutable() ];
        public override RelicModel ModelForOption => model.ToMutable();
    }
}

public class AncientOption<T>(int weight) : AncientOption(weight) where T : RelicModel
{
    /// <summary>
    /// Set this if relic needs to set up data based on current run state, eg. Sea Glass choosing a random other character.
    /// </summary>
    public Func<T, RelicModel>? ModelPrep { get; init; }
    public Func<T, IEnumerable<RelicModel>>? Variants { get; init; }

    private readonly T _model = ModelDb.Relic<T>();

    public override IEnumerable<RelicModel> AllVariants => Variants == null ? [_model.ToMutable()] : Variants(_model);
    public override RelicModel ModelForOption => ModelPrep == null ? _model.ToMutable() : ModelPrep(_model.ToMutable() as T ?? _model);
}
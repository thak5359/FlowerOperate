using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;

public class FertilizerMaker : MakerBaseModel, IMaker
{
    [SerializeField] private GameItem[] grassSlot = new GameItem[2];
    [SerializeField] private FlowerItem[] flowerSlot = new FlowerItem[3];

    private int ReturnTimes = 0;

    public override void SetGameItem(GameItem item)
    {
        if(item.SubType == ItemSubType.Grass)
        {
            GameItem grass = grassSlot.FirstOrDefault(element => element.CheckEmpty());
            if (grass != null)
                grass = item;
        }
        else if (item.SubType == ItemSubType.Flower)
        {
            FlowerItem flower = flowerSlot.FirstOrDefault(element => element.CheckEmpty());
            if (flower != null)
                flower = (FlowerItem)item;
        }
    }

    public async UniTask<GameItem> ReturnGameItemAsync()
    {
        ReturnTimes = CalculateMixableTimes();
        FertilizerItem outputFertilizer = MixFertilizerItem();
        if (ReturnTimes == 0)
        {
            Debug.LogError("반환할 아이템이 없습니다.");
            return null;
        }

        if (outputFertilizer == null)
            return null;

        await outputFertilizer.OnLoadAsync();
        SubElementCount(grassSlot);
        SubElementCount(flowerSlot);

        outputFertilizer.AddAmount(ReturnTimes * makerData.GetIngredientRatio.y);
        ReturnTimes = 0;
        return outputFertilizer;
    }

    private void SubElementCount(GameItem[] array)
    {
        foreach (var item in array)
            item.SubCount(ref ReturnTimes);
    }

    public int CalculateMixableTimes()
    {
        int times = 0;
        GameItem[] temp = new GameItem[5];
        temp = grassSlot;
        grassSlot.CopyTo(temp, 0);
        flowerSlot.CopyTo(temp, 2);

        times = temp.Min(item => item.Count);
        if (times == 0 || temp.Any(item => item.Id == 0))
            return 0;

        return times;
    }

    public FertilizerItem MixFertilizerItem()
    {
        // 전부다 신령초
        if(grassSlot.All(item => item.Id == 409006)) return null;

        bool isQualityFertilizer = flowerSlot.All(item => item.Color == flowerSlot[0].Color);
        int grade = (int)grassSlot.Average(item => item.Id - 409000);

        if(isQualityFertilizer)
        {
            return new FertilizerItem(301000 + grade, 0);
        }
        else if(grassSlot.Any(item => item.Id == 409006))
        {
            grade = (grade * 2) - 6;
            return new FertilizerItem(301010 + grade, 0);
        }
        return new FertilizerItem(301005 + grade, 0);
    }
}

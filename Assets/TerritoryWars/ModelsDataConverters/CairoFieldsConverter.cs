using Dojo.Starknet;

namespace TerritoryWars.ModelsDataConverters
{
    public static class CairoFieldsConverter
    {
        public static string FieldElement(FieldElement fieldElement)
        {
            return fieldElement.Hex();
        }
    }
}
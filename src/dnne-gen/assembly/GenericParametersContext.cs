using System.Reflection.Metadata;

namespace DNNE.Assembly;

public class GenericParametersContext
{
    private readonly MetadataReader metadataReader;
    private GenericParameterHandleCollection genericParameterHandleCollection;

    public GenericParametersContext(MetadataReader metadataReader, GenericParameterHandleCollection genericParameterHandleCollection)
    {
        this.metadataReader = metadataReader;
        this.genericParameterHandleCollection = genericParameterHandleCollection;
    }

    public GenericParameter? GetGenericParameterByIndex(int index)
    {
        if (index < 0 || index >= genericParameterHandleCollection.Count)
        {
            return null;
        }

        GenericParameterHandle handle = genericParameterHandleCollection[index];

        System.Reflection.Metadata.GenericParameter genericParameter = metadataReader.GetGenericParameter(handle);

        return new GenericParameter() {
            Index = genericParameter.Index,
            Name = metadataReader.GetString(genericParameter.Name),
            Attributes = genericParameter.Attributes,
        };
    }
}

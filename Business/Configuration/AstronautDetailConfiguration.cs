using StargateAPI.Business.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StargateAPI.Business.Configuration
{
    public class AstronautDetailConfiguration : IEntityTypeConfiguration<AstronautDetail>
    {
        public void Configure(EntityTypeBuilder<AstronautDetail> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
        }
    }
}
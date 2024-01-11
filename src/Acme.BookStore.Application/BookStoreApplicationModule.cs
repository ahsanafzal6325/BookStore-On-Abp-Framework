using Acme.BookStore.Books;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using StackExchange.Redis;
using System;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Entities.Caching;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace Acme.BookStore;

[DependsOn(
    typeof(BookStoreDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(BookStoreApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class BookStoreApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<BookStoreApplicationModule>();
        });
        context.Services.AddEntityCache<Book, Guid>();
        context.Services.AddEntityCache<Book, BookDto, Guid>();
        Configure<RedisCacheOptions>(options =>
        {
            //options.ConfigurationOptions.
            //var configuration = ConfigurationOptions.Parse("localhost:6379");
            //var redisConnection = ConnectionMultiplexer.Connect(configuration);
            options.Configuration = "127.0.0.1:6379";
            options.InstanceName = "BookStoreInstance";
        });
        Configure<AbpDistributedCacheOptions>(options =>
        {

            options.GlobalCacheEntryOptions = new DistributedCacheEntryOptions()
            {
                // (Sliding)bu ayar ile 3 dakika bir talep edilmez ise cache ten silecek. Ancak her durumda 5 dakikada cacheten silmiş olacak.AbsoluteExpiration
                SlidingExpiration = TimeSpan.FromMinutes(3), // Sliding, belirtilen süre zarfında cache’den data talep edilirse eğer 3 dakika daha tutulma süresini uzatacak aksi taktirde datayı silecektir. 
                AbsoluteExpiration = DateTime.Now.AddMinutes(5) // Absolute, datanın cache’de tutulma süresini belirliyor
            };
            options.KeyPrefix = "EnzimWeb:";
        });
        //if (!HostingEnvironment.IsDevelopment())
        //{
        //    var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
        //    dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "ModeleName-Protection-Keys");
        //}
        //var configuration = ConfigurationOptions.Parse("localhost:6379");
        //var redisConnection = ConnectionMultiplexer.Connect(configuration);
    }
}

using BillPlatform.BLL;
using BillPlatform.DAL;
using BillPlatform.IBLL;
using BillPlatform.IDAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace BillPlatform.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<EFContext>();
            services.AddScoped<ITestDAL, TestDAL>();
            services.AddScoped<ITestBLL, TestBLL>();
            services.AddScoped<IIndUserDAL, IndUserDAL>();
            services.AddScoped<IIndUserBLL,IndUserBLL>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "记账平台",
                    Version = "V1.0.1",
                    Description = "对外API"
                });

                //配置api注释
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath, true); //添加控制器层注释（true表示显示控制器注释）
                //配置swagger权限
                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                //配置方法
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme()
                {
                    Description = "添加Bearer和一个空格",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
            });
            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    //验证密钥
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["JwtTokenManagement:secret"])),

                    //验证发行人
                    ValidateIssuer = true,
                    ValidIssuer = Configuration["JwtTokenManagement:issuer"],

                    //验证订阅人
                    ValidateAudience = true,
                    ValidAudience = Configuration["JwtTokenManagement:audience"],

                    //验证过期时间
                    RequireExpirationTime = true,

                    //验证生命周期
                    ValidateLifetime = true,

                    //缓存过期时间，即使配置了过期时间也要考虑过期时间+缓冲
                    ClockSkew = TimeSpan.Zero
                };
            });
            //解决json乱码
            services.AddControllersWithViews().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            });
            //解决viewbag乱码
            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BillPlatform.Service v1"));
            }

            app.UseRouting();

            //认证中间件
            app.UseAuthentication();
            //授权中间件
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

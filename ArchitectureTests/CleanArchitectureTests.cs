using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace GestaoRH.ArchitectureTests;

public class CleanArchitectureTests
{
    // Carrega a arquitetura do assembly do projeto. 
    // Usamos uma classe conhecida do projeto para localizar o assembly.
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(typeof(GestaoRH.Domain.Entities.Funcionario).Assembly)
        .Build();

    // Definição das Camadas (Layers) baseadas nos Namespaces (usando regex para incluir sub-namespaces)
    private readonly IObjectProvider<IType> DomainLayer = Types()
        .That()
        .ResideInNamespace("GestaoRH.Domain")
        .Or()
        .HaveFullNameContaining("GestaoRH.Domain.")
        .As("Camada de Domínio");

    private readonly IObjectProvider<IType> ApplicationLayer = Types()
        .That()
        .ResideInNamespace("GestaoRH.Application")
        .Or()
        .HaveFullNameContaining("GestaoRH.Application.")
        .As("Camada de Aplicação");

    private readonly IObjectProvider<IType> InfrastructureLayer = Types()
        .That()
        .ResideInNamespace("GestaoRH.Infrastructure")
        .Or()
        .HaveFullNameContaining("GestaoRH.Infrastructure.")
        .As("Camada de Infraestrutura");

    private readonly IObjectProvider<IType> ApiLayer = Types()
        .That()
        .ResideInNamespace("GestaoRH.API")
        .Or()
        .HaveFullNameContaining("GestaoRH.API.")
        .As("Camada API (Apresentação)");

    [Fact]
    public void DomainShouldNotDependOnOtherLayers()
    {
        // O Domínio é o centro e não deve conhecer nenhuma outra camada
        IArchRule rule = Types()
            .That()
            .Are(DomainLayer)
            .Should()
            .NotDependOnAny(ApplicationLayer)
            .AndShould()
            .NotDependOnAny(InfrastructureLayer)
            .AndShould()
            .NotDependOnAny(ApiLayer)
            .Because("o Domínio deve ser independente de detalhes de implementação e lógica de aplicação.");

        rule.Check(Architecture);
    }

    [Fact]
    public void ApplicationShouldNotDependOnInfrastructureOrApi()
    {
        // A Aplicação só deve depender do Domínio
        IArchRule rule = Types()
            .That()
            .Are(ApplicationLayer)
            .Should()
            .NotDependOnAny(InfrastructureLayer)
            .AndShould()
            .NotDependOnAny(ApiLayer)
            .Because(
                "a camada de Aplicação não deve conhecer detalhes de persistência ou protocolos de comunicação (Web/API).");

        rule.Check(Architecture);
    }

    [Fact]
    public void InfrastructureShouldNotDependOnApi()
    {
        // A Infraestrutura pode depender de Application e Domain, mas nunca da API
        IArchRule rule = Types()
            .That()
            .Are(InfrastructureLayer)
            .Should()
            .NotDependOnAny(ApiLayer)
            .Because(
                "a camada de Infraestrutura lida com dados e serviços externos, e não deve ter dependências com a camada de apresentação.");

        rule.Check(Architecture);
    }

    [Fact]
    public void ClassesShouldBeInCorrectNamespace()
    {
        // Exemplo de regra para garantir que entidades fiquem no namespace correto
        IArchRule rule = Classes()
            .That()
            .ResideInNamespace("GestaoRH.Domain.Entities")
            .Should()
            .Be(DomainLayer)
            .Because("entidades devem residir na camada de domínio.");

        rule.Check(Architecture);
    }
}
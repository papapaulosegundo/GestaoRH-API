# Testes de Arquitetura (ArchUnitNET)

Este projeto utiliza o **ArchUnitNET** para garantir que a estrutura do código siga os princípios da **Clean Architecture** e **Vertical Slice Architecture**. Os testes de arquitetura automatizam a verificação de regras de dependência entre camadas, impedindo que acoplamentos indesejados sejam introduzidos no código.

## 1. Como os Testes Funcionam

Os testes estão localizados na pasta `ArchitectureTests/` e utilizam o framework `ArchUnitNET.xUnit`. Eles analisam o assembly compilado do projeto e aplicam regras fluentes para validar as relações entre classes e namespaces.

### Camadas Definidas (Layers)
As camadas são mapeadas com base nos namespaces do projeto:
*   **Domínio (`Domain`)**: `GestaoRH.Domain`
*   **Aplicação (`Application`)**: `GestaoRH.Application`
*   **Infraestrutura (`Infrastructure`)**: `GestaoRH.Infrastructure`
*   **API / Apresentação (`API`)**: `GestaoRH.API`

## 2. Regras de Dependência (Clean Architecture)

Para manter o núcleo do negócio isolado de detalhes técnicos, as seguintes regras são impostas:

1.  **Independência do Domínio**: A camada de Domínio não pode depender de nenhuma outra camada (Aplicação, Infraestrutura ou API).
2.  **Isolamento da Aplicação**: A camada de Aplicação pode depender do Domínio, mas não pode depender da Infraestrutura ou da API.
3.  **Isolamento da Infraestrutura**: A camada de Infraestrutura pode depender do Domínio e da Aplicação, mas nunca da API.
4.  **API como Consumidora**: A camada de API (Apresentação) pode depender de todas as camadas inferiores para orquestrar as requisições.

## 3. Como Executar os Testes

Você pode executar os testes de arquitetura via linha de comando ou através do Test Explorer do seu IDE (Visual Studio, Rider, VS Code).

### Via Terminal (dotnet CLI)
Para rodar apenas os testes de arquitetura:
```bash
dotnet test --filter "CleanArchitectureTests"
```

Para rodar todos os testes do projeto (incluindo unitários, se houver):
```bash
dotnet test
```

## 4. O que fazer quando um teste falha?

Se um teste de arquitetura falhar, isso geralmente significa que uma regra de dependência foi violada. 

**Exemplo de falha comum:**
> *Error: LoginHandler does depend on "GestaoRH.Infrastructure.Security.Jwt"*

**Como resolver:**
1.  **Identifique a dependência**: No exemplo acima, a camada de Aplicação está tentando usar diretamente uma classe da Infraestrutura.
2.  **Crie uma Abstração**: Crie uma `interface` na camada de Domínio ou Aplicação.
3.  **Inverta a Dependência (DIP)**: Faça a classe da Infraestrutura implementar essa interface e injete a interface no componente da Aplicação via construtor.

---
*Estes testes garantem que o sistema permaneça manutenível e escalável a longo prazo.*

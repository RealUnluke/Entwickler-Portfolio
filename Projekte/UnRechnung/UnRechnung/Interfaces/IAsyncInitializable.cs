using System.Threading.Tasks;

namespace UnRechnung.Interfaces
{
  internal interface IAsyncInitializable
  {
    Task InitializeAsync();
  }
}

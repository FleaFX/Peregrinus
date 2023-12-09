using System.Threading.Tasks;

namespace Peregrinus.Model; 

/// <summary>
/// Represents delegate that performs an actual migration operation.
/// </summary>
public delegate Task<int> AsyncMigrationOperation();
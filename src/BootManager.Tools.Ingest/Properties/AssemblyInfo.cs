using System.Runtime.CompilerServices;

// Make internals visible to unit tests so pure Ingest helpers can be tested without widening public API.
[assembly: InternalsVisibleTo("BootManager.UnitTests")]

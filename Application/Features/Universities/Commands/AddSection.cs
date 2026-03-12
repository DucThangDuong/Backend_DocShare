using Application.Common;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Features.Universities.Commands;

public record AddSectionCommand(int UniversityId, string Name);

public class AddSectionHandler
{
    private readonly IUniversities _repo;

    public AddSectionHandler(IUniversities repo)
    {
        _repo = repo;
    }

    public async Task<Result<UniversitySection>> HandleAsync(AddSectionCommand cmd, CancellationToken ct = default)
    {
        if (!await _repo.ExistsAsync(cmd.UniversityId))
            return Result<UniversitySection>.Failure("Không tìm thấy trường đại học.", 404);

        var section = await _repo.AddSectionToUniversityAsync(cmd.UniversityId, cmd.Name);
        await _repo.SaveChangesAsync();
        return Result<UniversitySection>.Success(section);
    }
}

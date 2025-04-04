﻿using HigLabo.Net.OAuth;

namespace HigLabo.Net.Microsoft;

/// <summary>
/// https://learn.microsoft.com/en-us/graph/api/resources/appmanagementpolicy?view=graph-rest-1.0
/// </summary>
public partial class AppManagementPolicy
{
    public string? Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public bool? IsEnabled { get; set; }
    public AppManagementConfiguration? Restrictions { get; set; }
    public DirectoryObject? AppliesTo { get; set; }
}

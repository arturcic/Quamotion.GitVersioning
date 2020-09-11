﻿using System;
using System.Collections.Generic;

namespace Quamotion.GitVersioning.Git
{
    public struct GitCommit : IEquatable<GitCommit>
    {
        public GitObjectId Tree { get; set; }
        public GitObjectId Sha { get; set; }
        public List<GitObjectId> Parents { get; set; }


        public override bool Equals(object obj)
        {
            if (obj is GitCommit)
            {
                return Equals((GitCommit)obj);
            }

            return false;
        }

        public bool Equals(GitCommit other)
        {
            return this.Sha.Equals(other.Sha);
        }

        public static bool operator ==(GitCommit left, GitCommit right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GitCommit left, GitCommit right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            return this.Sha.GetHashCode();
        }

        public override string ToString()
        {
            return $"Git Commit: {this.Sha}";
        }
    }
}

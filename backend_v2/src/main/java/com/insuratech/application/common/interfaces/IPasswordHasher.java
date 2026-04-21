package com.insuratech.application.common.interfaces;

public interface IPasswordHasher {
    String hash(String rawPassword);
    boolean verify(String rawPassword, String hash);
}

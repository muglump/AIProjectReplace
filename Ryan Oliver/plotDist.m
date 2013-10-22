function [ sol ] = plotDist(fileName )
%PLOTDIST 
    %clf;
    
    meanTrue = 0;
    standDevTrue = 0;
    meanFalse = 0;
    standDevFalse = 0;

    k = 1;
    % mean true, stand dev true, mean false, stand dev false
    fl = fopen(fileName);
    tline = fgets(fl);
    while ischar(tline)
        splitString = strsplit(tline,',');
        meanTrue(k) = str2double(splitString(1));
        standDevTrue(k) = str2double(splitString(2));
        meanFalse(k) = str2double(splitString(3));
        standDevFalse(k) = str2double(splitString(4));
        k = k + 1;
        tline = fgets(fl);
    end
    fclose(fl);
    k = k - 1;
    
    for j = [1,4,7,11,13,14,15,16,17,19,25,33,46,48,56,57,60,86,87,91,93,98,99,106,114,118,120,124,154,156,175]
        figure(j);
        hold on
        if standDevTrue(j) == 0
            if standDevFalse(j) == 0
                disp(j);
            else
                plotGaus(meanFalse(j),standDevFalse(j),2);
            end
        else
            if standDevFalse(j) == 0
                disp(j);
            else
                plotGaus(meanTrue(j),standDevTrue(j),1);
                plotGaus(meanFalse(j),standDevFalse(j),2);
            end
        end
        hold off
    end
end

function [] = plotGaus(mean,deviation,num)
    X = -1:0.001:1;
    Y = (1/(deviation*sqrt(2*pi)))*exp(-(power(X-mean,2))/(2*power(deviation,2)));
    
    if num == 1
        plot(X,Y,'b-');
    else
       plot(X,Y,'r-'); 
    end
end